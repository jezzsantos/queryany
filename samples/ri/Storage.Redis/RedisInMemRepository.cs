using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using QueryAny;
using QueryAny.Primitives;
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
        public const string TimeStampPropertyName = @"TimeStamp";
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

        public string Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            var id = this.idFactory.Create(entity);
            entity.Identify(id);

            var key = CreateRowKey(containerName, entity);
            client.SetRangeInHash(key, entity.ToContainerEntity());

            return id;
        }

        public void Remove<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                DeleteRow(client, containerName, id);
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            var key = CreateRowKey(containerName, id);
            var thing = RetrieveContainerEntitySafe<TEntity>(client, key);

            return thing.Entity;
        }

        public void Replace<TEntity>(string containerName, string id, TEntity entity)
            where TEntity : IPersistableEntity, new()
        {
            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                var key = CreateRowKey(containerName, id);
                var keyValues = entity.ToContainerEntity();
                client.Remove(key);
                client.SetRangeInHash(key, keyValues);
            }
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
                        .ToDictionary(e => e.Key, e => e.Value.Dehydrate());

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
                .Select(rowKey => RetrieveContainerEntitySafe<TEntity>(client, rowKey))
                .OrderBy(e => e.TimeStamp)
                .Select(e => e.Entity)
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
                    rowKey => RetrieveContainerEntitySafe(client, rowKey, joinedEntity.Join.Right.EntityType).Entity);
        }

        private static string CreateRowKey<TEntity>(string containerName, TEntity entity)
            where TEntity : IPersistableEntity
        {
            return CreateRowKey(containerName, entity.Id);
        }

        private static string CreateRowKey(string containerName, string id)
        {
            return $"{CreateContainerKey(containerName)}{id}";
        }

        private static string CreateContainerKey(string containerName)
        {
            return $"{ContainerStorageKeyPrefix}{containerName}:";
        }

        private static RedisContainerEntity<TEntity> RetrieveContainerEntitySafe<TEntity>(IRedisClient client,
            string rowKey)
            where TEntity : IPersistableEntity, new()
        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return new RedisContainerEntity<TEntity>();
                }

                return containerEntityProperties.FromContainerProperties<TEntity>();
            }
            catch (Exception)
            {
                return new RedisContainerEntity<TEntity>();
            }
        }

        private static RedisContainerEntity<IPersistableEntity> RetrieveContainerEntitySafe(IRedisClient client,
            string rowKey, Type entityType)
        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                return containerEntityProperties.FromContainerProperties(entityType);
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

        private static void DeleteRow(IRedisClient client, string containerName, string id)
        {
            var key = CreateRowKey(containerName, id);
            client.Remove(key);
        }

        private static bool Exists(IRedisClient client, string containerName, string id)
        {
            var key = CreateRowKey(containerName, id);
            var count = client.GetHashKeys(key).Count;
            return count != 0;
        }

        private static bool Exists(IRedisClient client, string containerName)
        {
            var key = CreateRowKey(containerName, "*");
            var count = client.SearchKeys(key).Count;
            return count != 0;
        }

        private IRedisClient EnsureClient()
        {
            this.redisClient ??= new BasicRedisClientManager(this.connectionString);

            return this.redisClient.GetClient();
        }

        public class RedisContainerEntity<TEntity> where TEntity : IPersistableEntity
        {
            public TEntity Entity { get; set; }

            public DateTime TimeStamp { get; set; }
        }
    }

    public static class RedisStorageEntityExtensions
    {
        public static Dictionary<string, string> ToContainerEntity<TEntity>(this TEntity entity)
            where TEntity : IPersistableEntity
        {
            var entityProperties = entity.Dehydrate();
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

                        value = dateTime.ToUniversalTime().ToString("O");
                        break;

                    case DateTimeOffset dateTimeOffset:
                        if (dateTimeOffset == DateTimeOffset.MinValue)
                        {
                            dateTimeOffset = DateTimeOffset.MinValue.ToUniversalTime();
                        }

                        value = dateTimeOffset.ToUniversalTime().ToString("O");
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

            containerEntityProperties.Add(RedisInMemRepository.TimeStampPropertyName, DateTime.UtcNow.ToString("O"));
            containerEntityProperties.Remove(nameof(IPersistableEntity.EntityName));

            return containerEntityProperties;
        }

        public static RedisInMemRepository.RedisContainerEntity<TEntity> FromContainerProperties<TEntity>(
            this Dictionary<string, string> containerProperties)
            where TEntity : IPersistableEntity, new()
        {
            var targetType = new TEntity().GetType();
            var thing = containerProperties.FromContainerProperties(targetType);
            return new RedisInMemRepository.RedisContainerEntity<TEntity>
            {
                Entity = (TEntity) thing.Entity,
                TimeStamp = thing.TimeStamp
            };
        }

        public static RedisInMemRepository.RedisContainerEntity<IPersistableEntity> FromContainerProperties(
            this Dictionary<string, string> containerProperties,
            Type entityType)
        {
            var entityPropertyTypeInfo = entityType.GetProperties();
            var containerEntityProperties = containerProperties
                .Where(pair => entityPropertyTypeInfo.Any(prop => prop.Name.EqualsOrdinal(pair.Key)) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerProperty(entityPropertyTypeInfo.First(info =>
                        StringExtensions.EqualsIgnoreCase(info.Name, pair.Key)).PropertyType));

            var timeStamp = (DateTime) containerProperties[RedisInMemRepository.TimeStampPropertyName]
                .FromContainerProperty(typeof(DateTime));
            var entity = entityType.CreateInstance<IPersistableEntity>();
            entity.Rehydrate(containerEntityProperties);
            return new RedisInMemRepository.RedisContainerEntity<IPersistableEntity>
            {
                Entity = entity,
                TimeStamp = timeStamp
            };
        }

        private static object FromContainerProperty(this string propertyValue, Type targetType)
        {
            if (propertyValue == null
                || propertyValue == RedisInMemRepository.RedisHashNullToken)
            {
                return null;
            }
            
            if (targetType == typeof(bool))
            {
                return bool.Parse(propertyValue);
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.ParseExact(propertyValue, "O", null).ToUniversalTime();
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