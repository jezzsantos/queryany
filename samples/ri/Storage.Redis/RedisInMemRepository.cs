using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Redis;

namespace Storage.Redis
{
    public class RedisInMemRepository : IRepository
    {
        private const string ContainerStorageKeyPrefix = @"in-mem:containers:";
        public const string NullToken = @"null";
        private readonly string connectionString;
        private IRedisClientsManager redisClient;

        public RedisInMemRepository(string connectionString)
        {
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            this.connectionString = connectionString;
        }

        public int MaxQueryResults => 1000;

        public CommandEntity Add(string containerName, CommandEntity entity)

        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            entity.GuardAgainstNull(nameof(entity));

            var client = EnsureClient();

            var key = CreateRowKey(containerName, entity);
            client.SetRangeInHash(key, entity.ToContainerProperties());

            return Retrieve(containerName, entity.Id, entity.Metadata);
        }

        public void Remove(string containerName, string id)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));

            var client = EnsureClient();

            if (Exists(client, containerName, id))
            {
                DeleteRow(client, containerName, id);
            }
        }

        public CommandEntity Retrieve(string containerName, string id, RepositoryEntityMetadata metadata)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));
            metadata.GuardAgainstNull(nameof(metadata));

            var client = EnsureClient();

            var rowKey = CreateRowKey(containerName, id);

            var properties = GetEntityFromContainer(client, rowKey, metadata);
            return properties == null
                ? null
                : CommandEntity.FromCommandEntity(properties, metadata);
        }

        public CommandEntity Replace(string containerName, string id, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var client = EnsureClient();

            var keyValues = entity.ToContainerProperties();
            var key = CreateRowKey(containerName, id);
            client.Remove(key);
            client.SetRangeInHash(key, keyValues);

            return CommandEntity.FromCommandEntity(keyValues.FromContainerProperties(id, entity.Metadata), entity);
        }

        public long Count(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var client = EnsureClient();

            return GetRowKeys(client, containerName).Count;
        }

        public List<QueryEntity> Query<TQueryableEntity>(string containerName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            if (query == null || query.Options.IsEmpty)
            {
                return new List<QueryEntity>();
            }

            var client = EnsureClient();

            if (!Exists(client, containerName))
            {
                return new List<QueryEntity>();
            }

            var primaryEntities = QueryPrimaryEntities(client, containerName, query, metadata);

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
                        .ToDictionary(e => e.Id.ToString(), e => e.Properties);
                    var rightEntities = joinedContainer.Value.Collection
                        .ToDictionary(e => e.Key, e => e.Value.Properties);

                    primaryEntities = join
                        .JoinResults(leftEntities, rightEntities, metadata,
                            joinedEntity.Selects.ProjectSelectedJoinedProperties());
                }
            }

            return primaryEntities.CherryPickSelectedProperties(query, metadata);
        }

        public void DestroyAll(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var client = EnsureClient();

            var rowKeys = GetRowKeys(client, containerName);
            if (rowKeys.Any())
            {
                client.RemoveAll(rowKeys);
            }
        }

        public static RedisInMemRepository FromAppSettings(IAppSettings settings)
        {
            var localServerConnectionString = settings.GetString("RedisConnectionString");
            return new RedisInMemRepository(localServerConnectionString);
        }

        private List<QueryEntity> QueryPrimaryEntities<TQueryableEntity>(IRedisClient client, string containerName,
            QueryClause<TQueryableEntity> query, RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            var primaryEntities = GetRowKeys(client, containerName)
                .ToDictionary(key => key, key => GetEntityFromContainer(client, key, metadata));

            var orderByExpression = query.ToDynamicLinqOrderByClause();
            var primaryEntitiesDynamic = primaryEntities.AsQueryable()
                .OrderBy(orderByExpression)
                .Skip(query.GetDefaultSkip())
                .Take(query.GetDefaultTake(this));

            if (!query.Wheres.Any())
            {
                return primaryEntitiesDynamic
                    .Select(ped => QueryEntity.FromProperties(ped.Value, metadata))
                    .ToList();
            }

            var queryExpression = query.Wheres.ToDynamicLinqWhereClause();
            return primaryEntitiesDynamic
                .Where(queryExpression)
                .Select(ped => QueryEntity.FromProperties(ped.Value, metadata))
                .ToList();
        }

        private static Dictionary<string, QueryEntity> QueryJoiningContainer(IRedisClient client,
            QueriedEntity joinedEntity)
        {
            var containerName = joinedEntity.EntityName;
            if (!Exists(client, containerName))
            {
                return new Dictionary<string, QueryEntity>();
            }

            var metadata = RepositoryEntityMetadata.FromType(joinedEntity.Join.Right.EntityType);
            return GetRowKeys(client, containerName)
                .ToDictionary(rowKey => rowKey,
                    rowKey => QueryEntity.FromProperties(GetEntityFromContainer(client, rowKey, metadata), metadata));
        }

        private static string CreateRowKey(string containerName, CommandEntity entity)
        {
            return CreateRowKey(containerName, entity.Id);
        }

        private static string CreateRowKey(string containerName, string id)
        {
            return $"{CreateContainerKey(containerName)}{id}";
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

        private static IReadOnlyDictionary<string, object> GetEntityFromContainer(IRedisClient client,
            string rowKey, RepositoryEntityMetadata metadata)

        {
            try
            {
                var containerEntityProperties = client.GetAllEntriesFromHash(rowKey);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                var id = GetEntityIdFromRowKey(rowKey);

                return containerEntityProperties.FromContainerProperties(id, metadata);
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

    internal static class RedisInMemRepositoryExtensions
    {
        public static IReadOnlyDictionary<string, string> ToContainerProperties(this CommandEntity entity)
        {
            static bool IsNotExcluded(string propertyName)
            {
                var excludedPropertyNames = new[] {nameof(CommandEntity.Id)};

                return !excludedPropertyNames.Contains(propertyName);
            }

            var entityProperties = entity.Properties
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
                            dateTime = DateTime.MinValue;
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
                        value = RedisInMemRepository.NullToken;

                        break;

                    default:
                        value = pair.Value.ToString();

                        break;
                }

                containerEntityProperties.Add(pair.Key, value);
            }

            containerEntityProperties[nameof(CommandEntity.LastPersistedAtUtc)] = DateTime.UtcNow.ToIso8601();

            return containerEntityProperties;
        }

        public static IReadOnlyDictionary<string, object> FromContainerProperties(
            this IReadOnlyDictionary<string, string> containerProperties, string id,
            RepositoryEntityMetadata metadata)

        {
            var containerEntityProperties = containerProperties
                .Where(pair =>
                    metadata.HasType(pair.Key) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerProperty(metadata.GetPropertyType(pair.Key)));

            containerEntityProperties[nameof(CommandEntity.Id)] = id;

            return containerEntityProperties;
        }

        private static object FromContainerProperty(this string propertyValue, Type targetPropertyType)
        {
            if (propertyValue == null
                || propertyValue == RedisInMemRepository.NullToken)
            {
                return null;
            }

            if (targetPropertyType == typeof(bool) || targetPropertyType == typeof(bool?))
            {
                return bool.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(DateTime) || targetPropertyType == typeof(DateTime?))
            {
                return propertyValue.FromIso8601();
            }

            if (targetPropertyType == typeof(DateTimeOffset) || targetPropertyType == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.ParseExact(propertyValue, "O", null).ToUniversalTime();
            }

            if (targetPropertyType == typeof(Guid) || targetPropertyType == typeof(Guid?))
            {
                return Guid.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(double) || targetPropertyType == typeof(double?))
            {
                return double.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(long) || targetPropertyType == typeof(long?))
            {
                return long.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(int) || targetPropertyType == typeof(int?))
            {
                return int.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(byte[]))
            {
                return Convert.FromBase64String(propertyValue);
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