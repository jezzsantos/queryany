using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using Domain.Interfaces.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using QueryAny;
using QueryAny.Primitives;

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
        private CosmosClient client;
        private bool databaseExistenceHasBeenChecked;

        public AzureCosmosSqlApiRepository(string connectionString, string databaseName)
        {
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            databaseName.GuardAgainstNullOrEmpty(nameof(databaseName));
            this.connectionString = connectionString;
            this.databaseName = databaseName;
        }

        public int MaxQueryResults => 1000;

        public void Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            container.CreateItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();
        }

        public void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            if (Exists(container, id))
            {
                container.DeleteItemAsync<object>(id, new PartitionKey(id)).GetAwaiter()
                    .GetResult();
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, Identifier id, IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(container, id, domainFactory);

            return containerEntity;
        }

        public TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity,
            IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            var result = container.UpsertItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return ((JObject) result.Resource).FromContainerEntity<TEntity>(domainFactory);
        }

        public long Count(string containerName)
        {
            var container = EnsureContainer(containerName);

            try
            {
                var query = new QueryDefinition(@"SELECT VALUE COUNT(1) FROM c");
                var count = container.GetItemQueryIterator<int>(query);

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

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query,
            IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            try
            {
                var primaryResults = QueryPrimaryResults(query, container, containerName);

                var joinedTables = query.JoinedEntities
                    .Where(je => je.Join != null)
                    .ToDictionary(je => je.EntityName, je => new
                    {
                        Collection = QueryJoiningContainer(je,
                            primaryResults.Select(e => e[je.Join.Left.JoinedFieldName])),
                        JoinedEntity = je
                    });

                var primaryEntities = primaryResults
                    .ConvertAll(r => r.FromContainerEntity<TEntity>(domainFactory));

                if (joinedTables.Any())
                {
                    foreach (var joinedTable in joinedTables)
                    {
                        var joinedEntity = joinedTable.Value.JoinedEntity;
                        var join = joinedEntity.Join;
                        var leftEntities = primaryEntities.ToDictionary(e => e.Id, e => e.Dehydrate());
                        var rightEntities = joinedTable.Value.Collection.ToDictionary(
                            e => e[IdentifierPropertyName].Value<string>().ToIdentifier(),
                            e => e.FromContainerEntity(join.Right.EntityType, domainFactory)
                                .Dehydrate());

                        primaryEntities = join
                            .JoinResults<TEntity>(leftEntities, rightEntities, domainFactory,
                                joinedEntity.Selects.ProjectSelectedJoinedProperties());
                    }
                }

                return primaryEntities.CherryPickSelectedProperties(query, domainFactory,
                    new[] {IdentifierPropertyName});
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return new List<TEntity>();
                }

                throw;
            }
        }

        public void DestroyAll(string containerName)
        {
            var container = EnsureContainer(containerName);
            container.DeleteContainerAsync().GetAwaiter().GetResult();
            this.containers.Remove(containerName);
        }

        private List<JObject> QueryPrimaryResults<TEntity>(QueryClause<TEntity> query, Container container,
            string containerName)
            where TEntity : IPersistableEntity
        {
            var containerQuery = new QueryDefinition(query.ToAzureCosmosSqlApiQueryClause(containerName, this));

            return container.GetItemQueryIterator<object>(containerQuery)
                .ToList();
        }

        private List<JObject> QueryJoiningContainer(QueriedEntity joinedEntity,
            IEnumerable<JToken> propertyValues)
        {
            var containerName = joinedEntity.EntityName;
            var container = EnsureContainer(containerName);

            //HACK: pretty limited way to query lots of entities by individual Id
            var counter = 0;
            var query = propertyValues.Select(propertyValue => new WhereExpression
            {
                Condition = new WhereCondition
                {
                    FieldName = joinedEntity.Join.Right.JoinedFieldName,
                    Operator = ConditionOperator.EqualTo,
                    Value = propertyValue.ToObject<object>()
                },
                Operator = counter++ == 0
                    ? LogicalOperator.None
                    : LogicalOperator.Or
            }).ToList().ToAzureCosmosSqlApiWhereClause();

            var selectedPropertyNames = joinedEntity.Selects
                .Where(sel => sel.JoinedFieldName.HasValue())
                .Select(j => j.JoinedFieldName)
                .Concat(new[] {joinedEntity.Join.Right.JoinedFieldName, IdentifierPropertyName});
            var selectedFields = string.Join(", ",
                selectedPropertyNames
                    .Select(name => $"{PrimaryContainerAlias}.{name}"));

            var filter = $"SELECT {selectedFields} FROM {containerName} {PrimaryContainerAlias} WHERE ({query})";
            var containerQuery = new QueryDefinition(filter);

            return container.GetItemQueryIterator<object>(containerQuery)
                .ToList();
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

        private Container EnsureContainer(string containerName)
        {
            EnsureConnected();

            if (this.containers.ContainsKey(containerName))
            {
                return this.containers[containerName];
            }

            var container = this.client.GetDatabase(this.databaseName)
                .CreateContainerIfNotExistsAsync(containerName, DefaultPartitionKeyPath, DefaultThroughput)
                .GetAwaiter()
                .GetResult().Container;
            this.containers.Add(containerName, container);

            return this.containers[containerName];
        }

        private static bool Exists(Container container, Identifier id)
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

        private static TEntity RetrieveContainerEntitySafe<TEntity>(Container container, Identifier id,
            IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            try
            {
                var entity = container.ReadItemAsync<object>(id, new PartitionKey(id))
                    .GetAwaiter()
                    .GetResult();
                if (entity != null)
                {
                    return ((JObject) entity.Resource).FromContainerEntity<TEntity>(domainFactory);
                }

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static class AzureCosmosSqlApiExtensions
    {
        public static dynamic ToContainerEntity<TEntity>(this TEntity entity) where TEntity : IPersistableEntity
        {
            bool IsNotExcluded(string propertyName)
            {
                var excludedPropertyNames = new[] {nameof(IPersistableEntity.Id)};

                return !excludedPropertyNames.Contains(propertyName);
            }

            var containerEntity = new ExpandoObject();
            var containerEntityProperties = (IDictionary<string, object>) containerEntity;
            var entityProperties = entity.Dehydrate()
                .Where(pair => IsNotExcluded(pair.Key));
            foreach (var pair in entityProperties)
            {
                var value = pair.Value;
                if (value != null)
                {
                    if (value is IPersistableValueObject valueObject)
                    {
                        value = valueObject.Dehydrate();
                    }

                    if (value.GetType().IsComplexStorageType())
                    {
                        value = value.ToString();
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

            containerEntityProperties.Add(AzureCosmosSqlApiRepository.IdentifierPropertyName, entity.Id.ToString());

            var utcNow = DateTime.UtcNow;
            if (!entity.CreatedAtUtc.HasValue())
            {
                containerEntityProperties[nameof(IPersistableEntity.CreatedAtUtc)] = utcNow;
            }

            containerEntityProperties[nameof(IPersistableEntity.LastModifiedAtUtc)] = utcNow;

            return containerEntity;
        }

        public static TEntity FromContainerEntity<TEntity>(this JObject containerEntity,
            IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            return (TEntity) containerEntity.FromContainerEntity(typeof(TEntity),
                domainFactory);
        }

        public static IPersistableEntity FromContainerEntity(this JObject containerEntity, Type entityType,
            IDomainFactory domainFactory)
        {
            var entityPropertyTypeInfo = entityType.GetProperties();

            var containerEntityProperties = containerEntity.ToObject<Dictionary<string, object>>()
                .Where(pair =>
                    entityPropertyTypeInfo.Any(prop => prop.Name.EqualsOrdinal(pair.Key)) &&
                    pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerEntityProperty(entityPropertyTypeInfo
                        .First(prop => prop.Name.EqualsOrdinal(pair.Key)).PropertyType, domainFactory));

            var id = containerEntity[AzureCosmosSqlApiRepository.IdentifierPropertyName].ToString().ToIdentifier();
            containerEntityProperties[nameof(IIdentifiableEntity.Id)] = id;

            return containerEntityProperties.EntityFromContainerProperties(entityType, domainFactory);
        }

        private static object FromContainerEntityProperty(this object property, Type targetPropertyType,
            IDomainFactory domainFactory)
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

                    if (typeof(IPersistableValueObject).IsAssignableFrom(targetPropertyType))
                    {
                        return text.ValueObjectFromContainerProperty(targetPropertyType, domainFactory);
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

    // ReSharper disable once InconsistentNaming
    public static class AzureCosmosSqlApiQueryExtensions
    {
        public static string ToAzureCosmosSqlApiQueryClause<TEntity>(this QueryClause<TEntity> query,
            string containerName, IRepository repository) where TEntity : IPersistableEntity
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT {query.ToAzureCosmosSqlApiSelectClause()}");
            builder.Append($" FROM {containerName} {AzureCosmosSqlApiRepository.PrimaryContainerAlias}");

            var wheres = query.Wheres.ToAzureCosmosSqlApiWhereClause();
            if (wheres.HasValue())
            {
                builder.Append($" WHERE {wheres}");
            }

            var orderBy = query.ToAzureCosmosSqlApiOrderByClause();
            if (orderBy.HasValue())
            {
                builder.Append($" ORDER BY {orderBy}");
            }

            var skip = query.GetDefaultSkip();
            var take = query.GetDefaultTake(repository);
            builder.Append($" OFFSET {skip} LIMIT {take}");

            return builder.ToString();
        }

        private static string ToAzureCosmosSqlApiSelectClause<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IPersistableEntity
        {
            if (query.PrimaryEntity.Selects.Any())
            {
                var builder = new StringBuilder();

                builder.Append(
                    $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{AzureCosmosSqlApiRepository.IdentifierPropertyName}");
                foreach (var select in query.PrimaryEntity.Selects)
                {
                    builder.Append($", {AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{select.FieldName}");
                }

                return builder.ToString();
            }

            return @"*";
        }

        private static string ToAzureCosmosSqlApiOrderByClause<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IPersistableEntity
        {
            var orderBy = query.ResultOptions.OrderBy;
            var direction = orderBy.Direction == OrderDirection.Ascending
                ? "ASC"
                : "DESC";
            var by = query.GetDefaultOrdering();

            return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{by} {direction}";
        }

        public static string ToAzureCosmosSqlApiWhereClause(this IReadOnlyList<WhereExpression> wheres)
        {
            if (!wheres.Any())
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToAzureCosmosSqlApiWhereClause());
            }

            return builder.ToString();
        }

        private static string ToAzureCosmosSqlApiWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToAzureCosmosSqlApiOperatorClause()}{condition.ToAzureCosmosSqlApiConditionClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToAzureCosmosSqlApiOperatorClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToAzureCosmosSqlApiWhereClause()}");
                }

                builder.Append(")");

                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToAzureCosmosSqlApiOperatorClause(this LogicalOperator op)
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

        private static string ToAzureCosmosSqlApiConditionClause(this ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.EqualTo:
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

        private static string ToAzureCosmosSqlApiConditionClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName;
            if (fieldName.EqualsOrdinal(nameof(IPersistableEntity.Id)))
            {
                fieldName = AzureCosmosSqlApiRepository.IdentifierPropertyName;
            }

            var @operator = condition.Operator.ToAzureCosmosSqlApiConditionClause();

            var value = condition.Value;
            switch (value)
            {
                case string text:
                    return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{text}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{dateTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}'"
                        : $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{dateTime:yyyy-MM-ddTHH:mm:ssZ}'";

                case DateTimeOffset dateTimeOffset:
                    return dateTimeOffset == DateTimeOffset.MinValue
                        ? $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{dateTimeOffset:yyyy-MM-ddTHH:mm:ssK}'"
                        : $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{dateTimeOffset:O}'";

                case bool _:
                    return
                        $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} {value.ToString().ToLowerInvariant()}";

                case double _:
                case int _:
                case long _:
                    return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} {value}";

                case byte[] bytes:
                    return
                        $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{Convert.ToBase64String(bytes)}'";

                case Guid guid:
                    return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{guid:D}'";

                case null:
                    return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} null";

                default:
                    return value.ToOtherValueString(fieldName, @operator);
            }
        }

        private static string ToOtherValueString(this object value, string fieldName, string @operator)
        {
            if (value == null)
            {
                return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} null";
            }

            if (value is IPersistableValueObject valueObject)
            {
                return
                    $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{valueObject.Dehydrate()}'";
            }

            var escapedValue = value
                .ToString()
                .Replace("\"", "\\\"");

            return $"{AzureCosmosSqlApiRepository.PrimaryContainerAlias}.{fieldName} {@operator} '{escapedValue}'";
        }
    }
}