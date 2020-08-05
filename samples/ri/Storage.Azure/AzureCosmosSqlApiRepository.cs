using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
using Storage.Interfaces;

namespace Storage.Azure
{
    public class AzureCosmosSqlApiRepository : IRepository
    {
        internal const string IdentifierPropertyName = @"id";
        internal const string ContainerAlias = @"t";
        private const int DefaultThroughput = 400;
        private static readonly string DefaultPartitionKeyPath = $"/{IdentifierPropertyName}";
        private readonly string connectionString;
        private readonly Dictionary<string, Container> containers = new Dictionary<string, Container>();
        private readonly string databaseName;
        private readonly IIdentifierFactory idFactory;
        private CosmosClient client;
        private bool databaseExistenceHasBeenChecked;

        public AzureCosmosSqlApiRepository(string connectionString, string databaseName, IIdentifierFactory idFactory)
        {
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            databaseName.GuardAgainstNullOrEmpty(nameof(databaseName));
            idFactory.GuardAgainstNull(nameof(idFactory));
            this.connectionString = connectionString;
            this.databaseName = databaseName;
            this.idFactory = idFactory;
        }

        public int MaxQueryResults => 1000;

        public Identifier Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            var id = this.idFactory.Create(entity);
            entity.Identify(id);

            container.CreateItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return id;
        }

        public void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            if (Exists(container, id))
            {
                container.DeleteItemAsync<object>(id.Get(), new PartitionKey(id.Get())).GetAwaiter().GetResult();
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, Identifier id, EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe(container, id, entityFactory);

            return containerEntity;
        }

        public TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            var container = EnsureContainer(containerName);

            var result = container.UpsertItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return ((JObject) result.Resource).FromContainerEntity(entityFactory);
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
            EntityFactory<TEntity> entityFactory)
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
                    .ConvertAll(r => r.FromContainerEntity(entityFactory));

                if (joinedTables.Any())
                {
                    foreach (var joinedTable in joinedTables)
                    {
                        var joinedEntity = joinedTable.Value.JoinedEntity;
                        var join = joinedEntity.Join;
                        var leftEntities = primaryEntities.ToDictionary(e => e.Id, e => e.Dehydrate());
                        var rightEntities = joinedTable.Value.Collection.ToDictionary(
                            e => Identifier.Create(e[IdentifierPropertyName].Value<string>()),
                            e => e.FromContainerEntity(join.Right.EntityType, properties => entityFactory(properties))
                                .Dehydrate());

                        primaryEntities = join
                            .JoinResults<TEntity>(leftEntities, rightEntities,
                                joinedEntity.Selects.ProjectSelectedJoinedProperties());
                    }
                }

                return primaryEntities.CherryPickSelectedProperties(query, new[] {IdentifierPropertyName});
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
                    .Select(name => $"{ContainerAlias}.{name}"));

            var filter = $"SELECT {selectedFields} FROM {containerName} {ContainerAlias} WHERE ({query})";
            var containerQuery = new QueryDefinition(filter);

            return container.GetItemQueryIterator<object>(containerQuery)
                .ToList();
        }

        private void EnsureConnected()
        {
            if (this.client == null)
            {
                this.client = new CosmosClient(this.connectionString);
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
                var entity = container.ReadItemAsync<object>(id.Get(), new PartitionKey(id.Get())).GetAwaiter()
                    .GetResult();

                return entity != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static TEntity RetrieveContainerEntitySafe<TEntity>(Container container, Identifier id,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            try
            {
                var entity = container.ReadItemAsync<object>(id.Get(), new PartitionKey(id.Get())).GetAwaiter()
                    .GetResult();
                if (entity != null)
                {
                    return ((JObject) entity.Resource).FromContainerEntity(entityFactory);
                }

                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

    internal static class AzureCosmosSqlApiExtensions
    {
        public static TEntity FromContainerEntity<TEntity>(this JObject containerEntity,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity
        {
            return (TEntity) containerEntity.FromContainerEntity(typeof(TEntity),
                properties => entityFactory(properties));
        }

        public static IPersistableEntity FromContainerEntity(this JObject containerEntity, Type entityType,
            EntityFactory<IPersistableEntity> entityFactory)
        {
            var entityPropertyTypeInfo = entityType.GetProperties();

            var propertyValues = containerEntity.ToObject<Dictionary<string, object>>()
                .Where(pair =>
                    entityPropertyTypeInfo.Any(prop => prop.Name.EqualsOrdinal(pair.Key)) &&
                    pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromContainerEntityProperty(entityPropertyTypeInfo
                        .First(prop => prop.Name.EqualsOrdinal(pair.Key)).PropertyType));

            var id = containerEntity[AzureCosmosSqlApiRepository.IdentifierPropertyName].ToString();

            return propertyValues.CreateEntity(id, entityFactory);
        }

        private static object FromContainerEntityProperty(this object property, Type targetPropertyType)
        {
            var value = property;
            switch (value)
            {
                case JObject jObject:
                    if (targetPropertyType.IsComplexStorageType())
                    {
                        return jObject.ToObject(targetPropertyType);
                    }

                    return null;

                case string text:
                    if (targetPropertyType == typeof(byte[]))
                    {
                        if (text.HasValue())
                        {
                            return Convert.FromBase64String(text);
                        }

                        return default(byte[]);
                    }

                    if (targetPropertyType == typeof(Guid))
                    {
                        if (text.HasValue())
                        {
                            return Guid.Parse(text);
                        }

                        return Guid.Empty;
                    }

                    if (typeof(IPersistableValueType).IsAssignableFrom(targetPropertyType))
                    {
                        return text.ValueTypeFromContainerProperty(targetPropertyType);
                    }

                    if (targetPropertyType.IsComplexStorageType())
                    {
                        return text.ComplexTypeFromContainerProperty(targetPropertyType);
                    }

                    return text;

                case DateTime _:
                case DateTimeOffset _:
                case bool _:
                case int _:
                case long _:
                case double _:
                    return value;

                case null:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(property));
            }
        }

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
                    if (value is IPersistableValueType valueType)
                    {
                        value = valueType.Dehydrate();
                    }

                    if (value.GetType().IsComplexStorageType())
                    {
                        value = value.ToString();
                    }
                }

                if (value is DateTime dateTime)
                {
                    if (dateTime == DateTime.MinValue)
                    {
                        value = DateTime.MinValue.ToUniversalTime();
                    }
                }

                containerEntityProperties.Add(pair.Key, value);
            }

            containerEntityProperties.Add(AzureCosmosSqlApiRepository.IdentifierPropertyName, entity.Id.Get());

            var utcNow = DateTime.UtcNow;
            var createdDate = (DateTime) containerEntityProperties[nameof(IPersistableEntity.CreatedAtUtc)];
            if (!createdDate.HasValue())
            {
                containerEntityProperties[nameof(IPersistableEntity.CreatedAtUtc)] = utcNow;
            }

            containerEntityProperties[nameof(IPersistableEntity.LastModifiedAtUtc)] = utcNow;

            return containerEntity;
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

    public static class AzureCosmosSqlApiQueryExtensions
    {
        public static string ToAzureCosmosSqlApiQueryClause<TEntity>(this QueryClause<TEntity> query,
            string containerName, IRepository repository) where TEntity : IPersistableEntity
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT {query.ToAzureCosmosSqlApiSelectClause()}");
            builder.Append($" FROM {containerName} {AzureCosmosSqlApiRepository.ContainerAlias}");

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
                    $"{AzureCosmosSqlApiRepository.ContainerAlias}.{AzureCosmosSqlApiRepository.IdentifierPropertyName}");
                foreach (var select in query.PrimaryEntity.Selects)
                {
                    builder.Append($", {AzureCosmosSqlApiRepository.ContainerAlias}.{select.FieldName}");
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

            return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{by} {direction}";
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
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{text}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{dateTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}'"
                        : $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{dateTime:yyyy-MM-ddTHH:mm:ssZ}'";

                case DateTimeOffset dateTimeOffset:
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{dateTimeOffset:O}'";

                case bool _:
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} {value.ToString().ToLowerInvariant()}";

                case double _:
                case int _:
                case long _:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} {value}";

                case byte[] bytes:
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{Convert.ToBase64String(bytes)}'";

                case Guid guid:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{guid:D}'";

                case null:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} null";

                default:
                    return value.ToOtherValueString(fieldName, @operator);
            }
        }

        private static string ToOtherValueString(this object value, string fieldName, string @operator)
        {
            if (value == null)
            {
                return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} null";
            }

            if (value is IPersistableValueType valueType)
            {
                return
                    $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{valueType.Dehydrate()}'";
            }

            var escapedValue = value
                .ToString()
                .Replace("\"", "\\\"");

            return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {@operator} '{escapedValue}'";
        }
    }
}