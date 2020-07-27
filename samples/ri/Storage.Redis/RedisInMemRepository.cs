using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            idFactory.GuardAgainstNull(nameof(idFactory));
            this.connectionString = connectionString;
            this.idFactory = idFactory;
        }

        public void Dispose()
        {
            // No need to do anything here. IDisposable is used as a marker interface
        }

        public int MaxQueryResults => 1000;

        public Identifier Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity
        {
            var client = EnsureClient();

            var id = this.idFactory.Create(entity);
            entity.Identify(id);

            var key = CreateRowKey(containerName, entity);
            client.SetRangeInHash(key, entity.ToContainerEntity());

            return id;
        }

        public void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity
        {
            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                DeleteRow(client, containerName, id);
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, Identifier id, EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var client = EnsureClient();

            var rowKey = CreateRowKey(containerName, id);
            return FromContainerEntity(client, rowKey, entityFactory);
        }

        public TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                var keyValues = entity.ToContainerEntity();
                var key = CreateRowKey(containerName, id);
                client.Remove(key);
                client.SetRangeInHash(key, keyValues);
                return keyValues.FromContainerProperties(id.Get(), entityFactory);
            }

            return default;
        }

        public long Count(string containerName)
        {
            var client = EnsureClient();

            return GetRowKeys(client, containerName).Count;
        }

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var client = EnsureClient();

            if (!Exists(client, containerName))
            {
                return new List<TEntity>();
            }

            var primaryEntities = QueryPrimaryEntities(client, containerName, query, entityFactory);

            var joinedContainers = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.EntityName, je => new
                {
                    Collection = QueryJoiningContainer(client, je, properties => entityFactory(properties)),
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

        private List<TEntity> QueryPrimaryEntities<TEntity>(IRedisClient client, string containerName,
            QueryClause<TEntity> query, EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var primaryEntities = GetRowKeys(client, containerName)
                .Select(rowKey => FromContainerEntity(client, rowKey, entityFactory))
                .OrderBy(e => e.CreatedAtUtc)
                .ToList();

            var orderByExpression = query.ToDynamicLinqOrderByClause();
            var primaryEntitiesDynamic = primaryEntities.AsQueryable()
                .OrderBy(orderByExpression)
                .Skip(query.GetDefaultSkip())
                .Take(query.GetDefaultTake(this))
                .DistinctBy(query.GetDefaultDistinctBy());

            if (!query.Wheres.Any())
            {
                return primaryEntitiesDynamic
                    .ToList();
            }

            var queryExpression = query.Wheres.ToDynamicLinqWhereClause();
            primaryEntities = primaryEntitiesDynamic
                .Where(queryExpression)
                .ToList();

            return primaryEntities;
        }

        private static Dictionary<string, IPersistableEntity> QueryJoiningContainer(IRedisClient client,
            QueriedEntity joinedEntity, EntityFactory<IPersistableEntity> entityFactory)
        {
            var containerName = joinedEntity.EntityName;
            if (!Exists(client, containerName))
            {
                return new Dictionary<string, IPersistableEntity>();
            }

            return GetRowKeys(client, containerName)
                .ToDictionary(rowKey => rowKey,
                    rowKey => FromContainerEntity(client, rowKey, joinedEntity.Join.Right.EntityType, entityFactory));
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

        private static TEntity FromContainerEntity<TEntity>(IRedisClient client, string rowKey,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                var id = GetEntityIdFromRowKey(rowKey);
                return containerEntityProperties.FromContainerProperties(id, properties => entityFactory(properties));
            }
            catch (Exception)
            {
                return default;
            }
        }

        private static IPersistableEntity FromContainerEntity(IRedisClient client,
            string rowKey, Type entityType, EntityFactory<IPersistableEntity> entityFactory)
        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                var id = GetEntityIdFromRowKey(rowKey);
                return containerEntityProperties.FromContainerProperties(id, entityType, entityFactory);
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
                var excludedPropertyNames = new[] {nameof(IPersistableEntity.Id)};
                return !excludedPropertyNames.Contains(propertyName);
            }

            var entityProperties = entity.Dehydrate()
                .Where(pair => IsNotExcluded(pair.Key));
            var containerEntityProperties = new Dictionary<string, string>();
            foreach (var pair in entityProperties)
            {
                string value;
                var propertyType = pair.Value?.GetType();
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
                        value = text;
                        break;

                    case null:
                        value = RedisInMemRepository.RedisHashNullToken;
                        break;

                    default:
                        if (propertyType != null &&
                            typeof(IPersistableValueType).IsAssignableFrom(propertyType))
                        {
                            value = ((IPersistableValueType) pair.Value).Dehydrate();
                            break;
                        }

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

            return containerEntityProperties;
        }

        public static TEntity FromContainerProperties<TEntity>(
            this Dictionary<string, string> containerProperties, string entityId, EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var targetType = typeof(TEntity);
            return (TEntity) containerProperties.FromContainerProperties(entityId, targetType,
                properties => entityFactory(properties));
        }

        public static IPersistableEntity FromContainerProperties(
            this Dictionary<string, string> containerProperties, string id,
            Type entityType,
            EntityFactory<IPersistableEntity> entityFactory)
        {
            var entityPropertyTypeInfo = entityType.GetProperties();
            var containerEntityProperties = containerProperties
                .Where(pair =>
                    entityPropertyTypeInfo.Any(prop => prop.Name.EqualsOrdinal(pair.Key)) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerProperty(entityPropertyTypeInfo.First(info =>
                        StringExtensions.EqualsIgnoreCase(info.Name, pair.Key)).PropertyType));

            return containerEntityProperties.CreateEntity(id, entityFactory);
        }

        private static object FromContainerProperty(this string propertyValue, Type targetPropertyType)
        {
            if (propertyValue == null
                || propertyValue == RedisInMemRepository.RedisHashNullToken)
            {
                return null;
            }

            if (targetPropertyType == typeof(Identifier))
            {
                return Identifier.Create(propertyValue);
            }

            if (targetPropertyType == typeof(bool))
            {
                return bool.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(DateTime))
            {
                return propertyValue.FromIso8601();
            }

            if (targetPropertyType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.ParseExact(propertyValue, "O", null).ToUniversalTime();
            }

            if (targetPropertyType == typeof(Guid))
            {
                return Guid.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(byte[]))
            {
                return Convert.FromBase64String(propertyValue);
            }

            if (typeof(IPersistableValueType).IsAssignableFrom(targetPropertyType))
            {
                if (propertyValue.HasValue())
                {
                    return propertyValue.ValueTypeFromContainerProperty(targetPropertyType);
                }
            }

            if (targetPropertyType.IsComplexStorageType())
            {
                if (propertyValue.HasValue())
                {
                    return propertyValue.ComplexTypeFromContainerProperty(targetPropertyType);
                }
            }

            return propertyValue;
        }
    }
}