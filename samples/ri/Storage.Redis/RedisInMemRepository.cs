using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
using ServiceStack;
using ServiceStack.Redis;
using Storage.Interfaces;
using StringExtensions = ServiceStack.StringExtensions;

namespace Storage.Redis
{
    public class RedisInMemRepository : IRepository
    {
        private const string ContainerStorageKeyPrefix = @"in-mem:containers:";
        public const string RedisHashNullToken = @"null";
        private readonly string connectionString;
        private readonly IIdentifierFactory idFactory;
        private IRedisClientsManager redisClient;

        public RedisInMemRepository(string connectionString, IIdentifierFactory idFactory)
        {
            Guard.AgainstNullOrEmpty(() => connectionString, connectionString);
            Guard.AgainstNull(() => idFactory, idFactory);
            this.connectionString = connectionString;
            this.idFactory = idFactory;
        }

        public void Dispose()
        {
            // No need to do anything here. IDisposable is used as a marker interface
        }

        public Identifier Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            var id = this.idFactory.Create(entity);
            entity.Identify(id);

            var key = CreateRowKey(containerName, entity);
            client.SetRangeInHash(key, entity.ToContainerEntity());

            return id;
        }

        public void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                DeleteRow(client, containerName, id);
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            var rowKey = CreateRowKey(containerName, id);
            return FromContainerEntity<TEntity>(client, rowKey);
        }

        public TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity)
            where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                var keyValues = entity.ToContainerEntity();
                var key = CreateRowKey(containerName, id);
                client.Remove(key);
                client.SetRangeInHash(key, keyValues);
                return keyValues.FromContainerProperties<TEntity>(id.Get());
            }

            return default;
        }

        public long Count(string containerName)
        {
            var client = EnsureClient();

            return GetRowKeys(client, containerName).Count;
        }

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            if (!Exists(client, containerName))
            {
                return new List<TEntity>();
            }

            var primaryEntities = QueryPrimaryEntities(client, containerName, query);

            var joinedContainers = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.EntityName, je => new
                {
                    Collection = QueryJoiningContainer(client, je),
                    JoinedEntity = je
                });

            if (joinedContainers.Any())
            {
                foreach (var joinedContainer in joinedContainers)
                {
                    var joinedEntity = joinedContainer.Value.JoinedEntity;
                    var join = joinedEntity.Join;
                    var leftEntities = primaryEntities
                        .ToDictionary(e => e.Id, e => e.Dehydrate());
                    var rightEntities = joinedContainer.Value.Collection
                        .ToDictionary(e => Identifier.Create(e.Key), e => e.Value.Dehydrate());

                    primaryEntities = join
                        .JoinResults<TEntity>(leftEntities, rightEntities,
                            joinedEntity.Selects.ProjectSelectedJoinedProperties());
                }
            }

            return primaryEntities.CherryPickSelectedProperties(query);
        }

        public void DestroyAll(string containerName)
        {
            var client = EnsureClient();

            var rowKeys = GetRowKeys(client, containerName);
            if (rowKeys.Any())
            {
                client.RemoveAll(rowKeys);
            }
        }

        private static List<TEntity> QueryPrimaryEntities<TEntity>(IRedisClient client, string containerName,
            QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new()
        {
            var primaryEntities = GetRowKeys(client, containerName)
                .Select(rowKey => FromContainerEntity<TEntity>(client, rowKey))
                .OrderBy(e => e.CreatedAtUtc)
                .ToList();

            if (!query.Wheres.Any())
            {
                return primaryEntities
                    .ToList();
            }

            var queryExpression = query.Wheres.ToDynamicLinqWhereClause();
            primaryEntities = primaryEntities.AsQueryable()
                .Where(queryExpression)
                .ToList();

            return primaryEntities
                .ToList();
        }

        private static Dictionary<string, IPersistableEntity> QueryJoiningContainer(IRedisClient client,
            QueriedEntity<INamedEntity> joinedEntity)
        {
            var containerName = joinedEntity.EntityName;
            if (!Exists(client, containerName))
            {
                return new Dictionary<string, IPersistableEntity>();
            }

            return GetRowKeys(client, containerName)
                .ToDictionary(rowKey => rowKey,
                    rowKey => FromContainerEntity(client, rowKey, joinedEntity.Join.Right.EntityType));
        }

        private static string CreateRowKey<TEntity>(string containerName, TEntity entity)
            where TEntity : IPersistableEntity
        {
            return CreateRowKey(containerName, entity.Id);
        }

        private static string CreateRowKey(string containerName, Identifier id)
        {
            return $"{CreateContainerKey(containerName)}{id.Get()}";
        }

        private static string CreateRowKeyPattern(string containerName, string pattern)
        {
            return $"{CreateContainerKey(containerName)}{pattern}";
        }

        private static string CreateContainerKey(string containerName)
        {
            return $"{ContainerStorageKeyPrefix}{containerName}:";
        }

        private static string GetEntityIdFromRowKey(string rowKey)
        {
            return rowKey.Substring(rowKey.LastIndexOf(":", StringComparison.Ordinal) + 1);
        }

        private static TEntity FromContainerEntity<TEntity>(IRedisClient client, string rowKey)
            where TEntity : IPersistableEntity, new()
        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                var id = GetEntityIdFromRowKey(rowKey);
                return containerEntityProperties.FromContainerProperties<TEntity>(id);
            }
            catch (Exception)
            {
                return default;
            }
        }

        private static IPersistableEntity FromContainerEntity(IRedisClient client,
            string rowKey, Type entityType)
        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                var id = GetEntityIdFromRowKey(rowKey);
                return containerEntityProperties.FromContainerProperties(id, entityType);
            }
            catch (Exception)
            {
                return default;
            }
        }

        private static List<string> GetRowKeys(IRedisClient client, string containerName)
        {
            var pattern = "{0}*".Fmt(CreateContainerKey(containerName));
            return EnumerableExtensions.Safe(client.SearchKeys(pattern)).ToList();
        }

        private static void DeleteRow(IRedisClient client, string containerName, Identifier id)
        {
            var key = CreateRowKey(containerName, id);
            client.Remove(key);
        }

        private static bool Exists(IRedisClient client, string containerName, Identifier id)
        {
            var key = CreateRowKey(containerName, id);
            var count = client.GetHashKeys(key).Count;
            return count != 0;
        }

        private static bool Exists(IRedisClient client, string containerName)
        {
            var key = CreateRowKeyPattern(containerName, "*");
            var count = client.SearchKeys(key).Count;
            return count != 0;
        }

        private IRedisClient EnsureClient()
        {
            this.redisClient ??= new BasicRedisClientManager(this.connectionString);

            return this.redisClient.GetClient();
        }
    }

    public static class RedisStorageEntityExtensions
    {
        public static Dictionary<string, string> ToContainerEntity<TEntity>(this TEntity entity)
            where TEntity : IPersistableEntity
        {
            bool IsNotExcluded(string propertyName)
            {
                var excludedPropertyNames = new[] {nameof(IPersistableEntity.Id), nameof(INamedEntity.EntityName)};
                return !excludedPropertyNames.Contains(propertyName);
            }

            var entityProperties = entity.Dehydrate()
                .Where(pair => IsNotExcluded(pair.Key));
            var containerEntityProperties = new Dictionary<string, string>();
            foreach (var pair in entityProperties)
            {
                string value;
                switch (pair.Value)
                {
                    case DateTime dateTime:
                        if (!dateTime.HasValue())
                        {
                            dateTime = DateTime.MinValue.ToUniversalTime();
                        }

                        value = dateTime.ToIso8601();
                        break;

                    case DateTimeOffset dateTimeOffset:
                        if (dateTimeOffset == DateTimeOffset.MinValue)
                        {
                            dateTimeOffset = DateTimeOffset.MinValue.ToUniversalTime();
                        }

                        value = dateTimeOffset.ToIso8601();
                        break;

                    case Guid guid:
                        value = guid.ToString("D");
                        break;

                    case byte[] bytes:
                        value = Convert.ToBase64String(bytes);
                        break;

                    case string text:
                        value = text.GetType().IsComplexStorageType()
                            ? text.ToJson()
                            : text;
                        break;

                    case null:
                        value = RedisInMemRepository.RedisHashNullToken;
                        break;

                    default:
                        value = pair.Value.ToString();
                        break;
                }

                containerEntityProperties.Add(pair.Key, value);
            }

            var nowUtc = DateTime.UtcNow.ToIso8601();
            var createdAtUtc = (DateTime) containerEntityProperties[nameof(IModifiableEntity.CreatedAtUtc)]
                .FromContainerProperty(typeof(DateTime));
            if (!createdAtUtc.HasValue())
            {
                containerEntityProperties[nameof(IModifiableEntity.CreatedAtUtc)] = nowUtc;
            }

            containerEntityProperties[nameof(IModifiableEntity.LastModifiedAtUtc)] = nowUtc;
            containerEntityProperties.Remove(nameof(IPersistableEntity.EntityName));

            return containerEntityProperties;
        }

        public static TEntity FromContainerProperties<TEntity>(
            this Dictionary<string, string> containerProperties, string entityId)
            where TEntity : IPersistableEntity, new()
        {
            var targetType = new TEntity().GetType();
            return (TEntity) containerProperties.FromContainerProperties(entityId, targetType);
        }

        public static IPersistableEntity FromContainerProperties(
            this Dictionary<string, string> containerProperties, string entityId,
            Type entityType)
        {
            var entityPropertyTypeInfo = entityType.GetProperties();
            var containerEntityProperties = containerProperties
                .Where(pair =>
                    entityPropertyTypeInfo.Any(prop => prop.Name.EqualsOrdinal(pair.Key)) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerProperty(entityPropertyTypeInfo.First(info =>
                        StringExtensions.EqualsIgnoreCase(info.Name, pair.Key)).PropertyType));

            var entity = entityType.CreateInstance<IPersistableEntity>();
            entity.Rehydrate(containerEntityProperties);
            entity.Identify(Identifier.Create(entityId));
            return entity;
        }

        private static object FromContainerProperty(this string propertyValue, Type targetType)
        {
            if (propertyValue == null
                || propertyValue == RedisInMemRepository.RedisHashNullToken)
            {
                return null;
            }

            if (targetType == typeof(Identifier))
            {
                return Identifier.Create(propertyValue);
            }

            if (targetType == typeof(bool))
            {
                return bool.Parse(propertyValue);
            }

            if (targetType == typeof(DateTime))
            {
                return propertyValue.FromIso8601();
            }

            if (targetType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.ParseExact(propertyValue, "O", null).ToUniversalTime();
            }

            if (targetType == typeof(Guid))
            {
                return Guid.Parse(propertyValue);
            }

            if (targetType == typeof(byte[]))
            {
                return Convert.FromBase64String(propertyValue);
            }

            if (targetType.IsComplexStorageType())
            {
                if (propertyValue.HasValue())
                {
                    return JsonConvert.DeserializeObject(propertyValue, targetType);
                }
            }

            return propertyValue;
        }
    }
}