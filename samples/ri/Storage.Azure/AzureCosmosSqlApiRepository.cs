using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using Common;
using Domain.Interfaces.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using QueryAny;
using ServiceStack.Configuration;

namespace Storage.Azure
{
    // ReSharper disable once InconsistentNaming
    public class AzureCosmosSqlApiRepository : IRepository
    {
        internal const string IdentifierPropertyName = @"id";
        internal const string PrimaryContainerAlias = @"t";
        private const int DefaultThroughput = 400;
        private static readonly string DefaultPartitionKeyPath = $"/{IdentifierPropertyName}";
        private readonly string connectionString;
        private readonly Dictionary<string, Container> containers = new Dictionary<string, Container>();
        private readonly string databaseName;
        private readonly IRecorder recorder;
        private CosmosClient client;
        private bool databaseExistenceHasBeenChecked;

        private AzureCosmosSqlApiRepository(IRecorder recorder, string connectionString, string databaseName)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            databaseName.GuardAgainstNullOrEmpty(nameof(databaseName));
            this.recorder = recorder;
            this.connectionString = connectionString;
            this.databaseName = databaseName;
        }

        public int MaxQueryResults => 1000;

        public CommandEntity Add(string containerName, CommandEntity entity)

        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            entity.GuardAgainstNull(nameof(entity));

            var container = EnsureContainer(containerName);

            container.CreateItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return Retrieve(containerName, entity.Id, entity.Metadata);
        }

        public void Remove(string containerName, string id)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));

            var container = EnsureContainer(containerName);

            if (Exists(container, id))
            {
                container.DeleteItemAsync<object>(id, new PartitionKey(id)).GetAwaiter()
                    .GetResult();
            }
        }

        public CommandEntity Retrieve(string containerName, string id, RepositoryEntityMetadata metadata)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));
            metadata.GuardAgainstNull(nameof(metadata));

            var container = EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe(container, id, metadata);

            return containerEntity;
        }

        public CommandEntity Replace(string containerName, string id, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var container = EnsureContainer(containerName);

            var result = container.UpsertItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return CommandEntity.FromCommandEntity(((JObject) result.Resource).FromContainerEntity(entity.Metadata),
                entity);
        }

        public long Count(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var container = EnsureContainer(containerName);

            try
            {
                const string query = @"SELECT VALUE COUNT(1) FROM c";
                this.recorder.TraceInformation($"Executed SQL statement: {query}");
                var count = container.GetItemQueryIterator<int>(new QueryDefinition(query));

                return count.Count();
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return 0;
                }

                throw;
            }
        }

        public List<QueryEntity> Query<TQueryableEntity>(string containerName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            if (query == null || query.Options.IsEmpty)
            {
                return new List<QueryEntity>();
            }

            var container = EnsureContainer(containerName);

            try
            {
                List<QueryEntity> results;
                if (!query.HasAnyJoins())
                {
                    var containerQuery = query.ToCosmosSqlApiQueryClause(containerName, this);
                    this.recorder.TraceInformation($"Executed SQL statement: {containerQuery}");
                    results = container.GetItemQueryIterator<object>(new QueryDefinition(containerQuery)).ToList()
                        .Select(jObj => QueryEntity.FromProperties(jObj.FromContainerEntity(metadata), metadata))
                        .ToList();

                    return results;
                }

                //HACK: AzureCosmosSqlApiStorage does not support Joins, so we need to do the Join, OrderBy, Skip and Take in memory
                results = query.FetchAllIntoMemory(this, metadata,
                    () => QueryPrimaryEntities(containerName, container, query, metadata),
                    je => QueryJoiningContainer(je, metadata));

                return results;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return new List<QueryEntity>();
                }

                throw;
            }
        }

        public void DestroyAll(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var container = EnsureContainer(containerName);
            container.DeleteContainerAsync().GetAwaiter().GetResult();
            this.containers.Remove(containerName);
        }

        private Dictionary<string, IReadOnlyDictionary<string, object>> QueryPrimaryEntities<TQueryableEntity>(
            string containerName, Container container, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            var filter = query.PrimaryEntity.Selects.ToCosmosSqlApiSelectClause(containerName);
            this.recorder.TraceInformation($"Executed SQL statement: {filter}");
            return container.GetItemQueryIterator<object>(new QueryDefinition(filter))
                .ToList()
                .ToDictionary(jObj => jObj.Property(IdentifierPropertyName).Value.ToString(),
                    jObj => jObj.FromContainerEntity(metadata));
        }

        private Dictionary<string, IReadOnlyDictionary<string, object>> QueryJoiningContainer(QueriedEntity je,
            RepositoryEntityMetadata metadata)
        {
            var containerName = je.EntityName;
            var container = EnsureContainer(containerName);

            var selected = je.Selects
                .Where(sel => sel.JoinedFieldName.HasValue())
                .ToList();

            if (!selected.Any(sel => sel.EntityName.EqualsOrdinal(je.Join.Right.EntityName)))
            {
                selected.Add(new SelectDefinition(je.Join.Right.EntityName, je.Join.Right.JoinedFieldName, null, null));
            }

            var filter = selected.ToCosmosSqlApiSelectClause(containerName);
            this.recorder.TraceInformation($"Executed SQL statement: {filter}");
            return container.GetItemQueryIterator<object>(new QueryDefinition(filter))
                .ToList()
                .ToDictionary(jObj => jObj.Property(IdentifierPropertyName).Value.ToString(),
                    jObj => jObj.FromContainerEntity(metadata));
        }

        public static AzureCosmosSqlApiRepository FromSettings(IRecorder recorder, IAppSettings settings,
            string databaseName)
        {
            settings.GuardAgainstNull(nameof(settings));
            recorder.GuardAgainstNull(nameof(recorder));
            databaseName.GuardAgainstNullOrEmpty(nameof(databaseName));

            var accountKey = settings.GetString("AzureCosmosDbAccountKey");
            var hostName = settings.GetString("AzureCosmosDbHostName");
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";

            return new AzureCosmosSqlApiRepository(recorder, localEmulatorConnectionString, databaseName);
        }

        private void EnsureConnected()
        {
            if (this.client == null)
            {
                this.client = new CosmosClient(this.connectionString, new CosmosClientOptions
                {
                    Serializer = new CosmosJsonDotNetSerializer()
                });
            }

            if (!this.databaseExistenceHasBeenChecked)
            {
                this.databaseExistenceHasBeenChecked = true;
                this.client.CreateDatabaseIfNotExistsAsync(this.databaseName).GetAwaiter().GetResult();
            }
        }

        private Container EnsureContainer(string name)
        {
            name.ValidateCosmosContainerId();
            EnsureConnected();

            if (this.containers.ContainsKey(name))
            {
                return this.containers[name];
            }

            var container = this.client.GetDatabase(this.databaseName)
                .CreateContainerIfNotExistsAsync(name, DefaultPartitionKeyPath, DefaultThroughput)
                .GetAwaiter()
                .GetResult().Container;
            this.containers.Add(name, container);

            return this.containers[name];
        }

        private static bool Exists(Container container, string id)
        {
            try
            {
                var entity = container.ReadItemAsync<object>(id, new PartitionKey(id))
                    .GetAwaiter()
                    .GetResult();

                return entity != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static CommandEntity RetrieveContainerEntitySafe(Container container, string id,
            RepositoryEntityMetadata metadata)

        {
            try
            {
                var entity = container.ReadItemAsync<object>(id, new PartitionKey(id))
                    .GetAwaiter()
                    .GetResult();
                return CommandEntity.FromCommandEntity(
                    ((JObject) entity?.Resource)?.FromContainerEntity(metadata),
                    metadata);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static class AzureCosmosSqlApiRepositoryExtensions
    {
        public static dynamic ToContainerEntity(this CommandEntity entity)
        {
            bool IsNotExcluded(string propertyName)
            {
                var excludedPropertyNames = new[] {nameof(CommandEntity.Id)};

                return !excludedPropertyNames.Contains(propertyName);
            }

            var containerEntity = new ExpandoObject();
            var containerEntityProperties = (IDictionary<string, object>) containerEntity;
            var entityProperties = entity.Properties
                .Where(pair => IsNotExcluded(pair.Key));
            foreach (var pair in entityProperties)
            {
                var value = pair.Value;
                if (value != null)
                {
                    if (value is Enum)
                    {
                        value = value.ToString();
                    }

                    if (value.GetType().IsComplexStorageType())
                    {
                        value = value.ComplexTypeToContainerProperty();
                    }
                }

                if (value is DateTime dateTime)
                {
                    if (!dateTime.HasValue())
                    {
                        value = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                    }
                }

                containerEntityProperties.Add(pair.Key, value);
            }

            containerEntityProperties.Add(AzureCosmosSqlApiRepository.IdentifierPropertyName, entity.Id);
            containerEntityProperties[nameof(CommandEntity.LastPersistedAtUtc)] = DateTime.UtcNow;

            return containerEntity;
        }

        public static IReadOnlyDictionary<string, object> FromContainerEntity(this JObject containerEntity,
            RepositoryEntityMetadata metadata)

        {
            var containerEntityProperties = containerEntity.ToObject<Dictionary<string, object>>()
                .Where(pair =>
                    metadata.HasType(pair.Key) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerEntityProperty(metadata.GetPropertyType(pair.Key)));

            var id = containerEntity[AzureCosmosSqlApiRepository.IdentifierPropertyName].ToString();
            containerEntityProperties[nameof(CommandEntity.Id)] = id;

            return containerEntityProperties;
        }

        private static object FromContainerEntityProperty(this object property, Type targetPropertyType)
        {
            var value = property;
            switch (value)
            {
                case string text:
                    if (targetPropertyType == typeof(byte[]))
                    {
                        if (text.HasValue())
                        {
                            return Convert.FromBase64String(text);
                        }

                        return default(byte[]);
                    }

                    if (targetPropertyType == typeof(Guid) || targetPropertyType == typeof(Guid?))
                    {
                        if (text.HasValue())
                        {
                            return Guid.Parse(text);
                        }

                        return Guid.Empty;
                    }

                    if (targetPropertyType.IsEnum || targetPropertyType.IsNullableEnum())
                    {
                        if (targetPropertyType.IsEnum)
                        {
                            return Enum.Parse(targetPropertyType, text, true);
                        }

                        if (targetPropertyType.IsNullableEnum())
                        {
                            if (text.HasValue())
                            {
                                return targetPropertyType.ParseNullable(text);
                            }
                            return null;
                        }
                    }

                    if (targetPropertyType.IsComplexStorageType())
                    {
                        return text.ComplexTypeFromContainerProperty(targetPropertyType);
                    }

                    return text;

                case DateTimeOffset dateTimeOffset:
                    if (targetPropertyType == typeof(DateTime) || targetPropertyType == typeof(DateTime?))
                    {
                        var dateTime = dateTimeOffset.UtcDateTime;
                        return !dateTime.HasValue()
                            ? DateTime.MinValue
                            : dateTime;
                    }
                    else
                    {
                        return dateTimeOffset;
                    }

                case bool _:
                case int _:
                case long _:
                case double _:
                    if (targetPropertyType == typeof(int) || targetPropertyType == typeof(int?))
                    {
                        return Convert.ToInt32(value);
                    }
                    if (targetPropertyType == typeof(long) || targetPropertyType == typeof(long?))
                    {
                        return Convert.ToInt64(value);
                    }
                    if (targetPropertyType == typeof(double) || targetPropertyType == typeof(double?))
                    {
                        return Convert.ToDouble(value);
                    }
                    return value;

                case null:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(property));
            }
        }

        public static long Count(this FeedIterator<int> iterator)
        {
            var count = 0;
            while (iterator.HasMoreResults)
            {
                var currentResultSet = iterator.ReadNextAsync().GetAwaiter().GetResult();
                count += currentResultSet.Sum();
            }

            return count;
        }

        public static List<JObject> ToList(this FeedIterator<object> iterator)
        {
            var results = new List<JObject>();
            while (iterator.HasMoreResults)
            {
                var currentResultSet = iterator.ReadNextAsync().GetAwaiter().GetResult();
                results.AddRange(currentResultSet.Resource.ToList().ConvertAll(x => (JObject) x));
            }

            return results;
        }
    }

    internal static class CosmosSqlApiQueryExtensions
    {
        public static string ToCosmosSqlApiSelectClause(
            this IEnumerable<SelectDefinition> selects, string containerName)
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT {selects.ToSelectClause()}");
            builder.Append($" FROM {containerName} {AzureCosmosSqlApiRepository.PrimaryContainerAlias}");

            return builder.ToString();
        }

        public static string ToCosmosSqlApiQueryClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query,
            string containerName, IRepository repository) where TQueryableEntity : IQueryableEntity
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT {query.PrimaryEntity.Selects.ToSelectClause()}");
            builder.Append($" FROM {containerName} {AzureCosmosSqlApiRepository.PrimaryContainerAlias}");

            var joins = query.JoinedEntities.ToJoinClause();
            if (joins.HasValue())
            {
                builder.Append($"{joins}");
            }

            var wheres = query.Wheres.ToWhereClause();
            if (wheres.HasValue())
            {
                builder.Append($" WHERE {wheres}");
            }

            var orderBy = query.ToOrderByClause();
            if (orderBy.HasValue())
            {
                builder.Append($" ORDER BY {orderBy}");
            }

            var skip = query.GetDefaultSkip();
            var take = query.GetDefaultTake(repository);
            builder.Append($" OFFSET {skip} LIMIT {take}");

            return builder.ToString();
        }

        private static string ToSelectClause(this IEnumerable<SelectDefinition> selects)
        {
            var definitions = selects
                .Safe().ToList();
            if (definitions.HasAny())
            {
                var builder = new StringBuilder();

                builder.Append(
                    $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{AzureCosmosSqlApiRepository.IdentifierPropertyName}");
                foreach (var select in definitions)
                {
                    if (!IsIdProperty(select.FieldName))
                    {
                        builder.Append($", {AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{select.FieldName}");
                    }
                }

                return builder.ToString();
            }

            return @"*";
        }

        private static string ToOrderByClause<TQueryableEntity>(
            this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity
        {
            var orderBy = query.ResultOptions.OrderBy;
            var direction = orderBy.Direction == OrderDirection.Ascending
                ? "ASC"
                : "DESC";
            var by = query.GetDefaultOrdering();
            if (IsIdProperty(by))
            {
                by = AzureCosmosSqlApiRepository.IdentifierPropertyName;
            }

            return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{by} {direction}";
        }

        private static string ToJoinClause(this IReadOnlyList<QueriedEntity> joinedEntities)
        {
            if (!joinedEntities.Any())
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var entity in joinedEntities)
            {
                builder.Append(entity.Join.ToJoinClause());
            }

            return builder.ToString();
        }

        private static string ToJoinClause(this JoinDefinition join)
        {
            var joinType = join.Type.ToJoinType();

            return
                $" {joinType} JOIN {join.Right.EntityName} ON {AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{join.Left.JoinedFieldName}={join.Right.EntityName}.{join.Right.JoinedFieldName}";
        }

        private static string ToJoinType(this JoinType type)
        {
            switch (type)
            {
                case JoinType.Inner:
                    return "INNER";
                case JoinType.Left:
                    return "LEFT";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static string ToWhereClause(this IReadOnlyList<WhereExpression> wheres)
        {
            if (!wheres.Any())
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToWhereClause());
            }

            return builder.ToString();
        }

        private static string ToWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToOperatorClause()}{condition.ToConditionClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToOperatorClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToWhereClause()}");
                }

                builder.Append(")");

                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToOperatorClause(this LogicalOperator op)
        {
            switch (op)
            {
                case LogicalOperator.None:
                    return string.Empty;
                case LogicalOperator.And:
                    return " AND ";
                case LogicalOperator.Or:
                    return " OR ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToConditionClause(this ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.EqualTo:
                case ConditionOperator.Like:
                    return "=";
                case ConditionOperator.GreaterThan:
                    return ">";
                case ConditionOperator.GreaterThanEqualTo:
                    return ">=";
                case ConditionOperator.LessThan:
                    return "<";
                case ConditionOperator.LessThanEqualTo:
                    return "<=";
                case ConditionOperator.NotEqualTo:
                    return "<>";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToConditionClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName;
            if (IsIdProperty(fieldName))
            {
                fieldName = AzureCosmosSqlApiRepository.IdentifierPropertyName;
            }

            var @operator = condition.Operator.ToConditionClause();

            var value = condition.Value;
            var fieldNameExpression = $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}[\"{fieldName}\"]";
            switch (value)
            {
                case string text:
                    if (condition.Operator == ConditionOperator.Like)
                    {
                        return $"{fieldNameExpression} LIKE '%{text}%'";
                    }
                    return $"{fieldNameExpression} {@operator} '{text}'";

                case Enum @enum:
                    return $"{fieldNameExpression} {@operator} '{@enum.ToString()}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? $"{fieldNameExpression} {@operator} '{dateTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}'"
                        : $"{fieldNameExpression} {@operator} '{dateTime:yyyy-MM-ddTHH:mm:ssZ}'";

                case DateTimeOffset dateTimeOffset:
                    return dateTimeOffset == DateTimeOffset.MinValue
                        ? $"{fieldNameExpression} {@operator} '{dateTimeOffset:yyyy-MM-ddTHH:mm:ssK}'"
                        : $"{fieldNameExpression} {@operator} '{dateTimeOffset:O}'";

                case bool _:
                    return
                        $"{fieldNameExpression} {@operator} {value.ToString().ToLowerInvariant()}";

                case double _:
                case int _:
                case long _:
                    return $"{fieldNameExpression} {@operator} {value}";

                case byte[] bytes:
                    return
                        $"{fieldNameExpression} {@operator} '{Convert.ToBase64String(bytes)}'";

                case Guid guid:
                    return $"{fieldNameExpression} {@operator} '{guid:D}'";

                case null:
                    return $"{fieldNameExpression} {@operator} null";

                default:
                    return value.ToWhereConditionOtherValueString(fieldName, @operator);
            }
        }

        private static string ToWhereConditionOtherValueString(this object value, string fieldName, string @operator)
        {
            var fieldNameExpression = $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}[\"{fieldName}\"]";
            if (value == null)
            {
                return $"{fieldNameExpression} {@operator} null";
            }

            if (value is IPersistableValueObject valueObject)
            {
                return
                    $"{fieldNameExpression} {@operator} '{valueObject.Dehydrate()}'";
            }

            var escapedValue = value
                .ToString()
                .Replace("\"", "\\\"");

            return $"{fieldNameExpression} {@operator} '{escapedValue}'";
        }

        private static bool IsIdProperty(string fieldName)
        {
            return fieldName.EqualsIgnoreCase(AzureCosmosSqlApiRepository.IdentifierPropertyName);
        }
    }
}