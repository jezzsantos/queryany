using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;
using StringExtensions = ServiceStack.StringExtensions;

namespace Storage.Azure
{
    public class AzureCosmosSqlApiRepository : IRepository
    {
        internal const string IdentifierPropertyName = @"id";
        internal const string ContainerAlias = @"t";
        private const int DefaultThroughput = 400;

        internal static readonly string[] CosmosReservedPropertyNames =
            {"_rid", "_self", "_etag", "_attachments", "_ts"};

        private static readonly string DefaultPartitionKeyPath = $"/{IdentifierPropertyName}";
        private readonly string connectionString;
        private readonly Dictionary<string, Container> containers = new Dictionary<string, Container>();
        private readonly string databaseName;
        private readonly IIdentifierFactory idFactory;
        private CosmosClient client;
        private bool databaseExistenceHasBeenChecked;

        public AzureCosmosSqlApiRepository(string connectionString, string databaseName, IIdentifierFactory idFactory)
        {
            Guard.AgainstNullOrEmpty(() => connectionString, connectionString);
            Guard.AgainstNullOrEmpty(() => databaseName, databaseName);
            Guard.AgainstNull(() => idFactory, idFactory);
            this.connectionString = connectionString;
            this.databaseName = databaseName;
            this.idFactory = idFactory;
        }

        public void Dispose()
        {
            // No need to do anything here. IDisposable is used as a marker interface
        }

        public string Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new()
        {
            var container = EnsureContainer(containerName);

            var id = this.idFactory.Create(entity);
            entity.Identify(id);

            container.CreateItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return id;
        }

        public void Remove<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            var container = EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(container, id);
            if (containerEntity != null)
            {
                container.DeleteItemAsync<object>(id, new PartitionKey(id)).GetAwaiter().GetResult();
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            var container = EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(container, id);

            return containerEntity;
        }

        public void Replace<TEntity>(string containerName, string id, TEntity entity)
            where TEntity : IPersistableEntity, new()
        {
            var container = EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(container, id);
            if (containerEntity != null)
            {
                container.UpsertItemAsync(entity.ToContainerEntity());
            }
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

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new()
        {
            var container = EnsureContainer(containerName);

            try
            {
                var containerQuery = new QueryDefinition(query.ToAzureCosmosSqlApiWhereClause(containerName));
                var primaryResults = container.GetItemQueryIterator<object>(containerQuery)
                    .ToList();

                var joinedTables = query.JoinedEntities
                    .Where(je => je.Join != null)
                    .ToDictionary(je => je.EntityName, je => new
                    {
                        Collection = QueryJoiningContainer(je,
                            primaryResults.Select(e => e[je.Join.Left.JoinedFieldName])),
                        JoinedEntity = je
                    });

                var primaryEntities = primaryResults
                    .ConvertAll(r => r.FromContainerEntity<TEntity>());

                if (joinedTables.Any())
                {
                    foreach (var joinedTable in joinedTables)
                    {
                        var joinedEntity = joinedTable.Value.JoinedEntity;
                        var join = joinedEntity.Join;
                        var leftEntities = primaryEntities.ToDictionary(e => e.Id, e => e.Dehydrate());
                        var rightEntities = joinedTable.Value.Collection.ToDictionary(
                            e => e[nameof(IPersistableEntity.Id)].Value<string>(),
                            e => e.FromContainerEntity(join.Right.EntityType).Dehydrate());

                        primaryEntities = join
                            .JoinResults<TEntity>(leftEntities, rightEntities,
                                joinedEntity.Selects.ProjectSelectedJoinedProperties());
                    }
                }

                return primaryEntities.CherryPickSelectedProperties(query);
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

        private List<JObject> QueryJoiningContainer(QueriedEntity<INamedEntity> joinedEntity,
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
                .Concat(new[] {joinedEntity.Join.Right.JoinedFieldName, nameof(IPersistableEntity.Id)});
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

        private static TEntity RetrieveContainerEntitySafe<TEntity>(Container container, string id)
            where TEntity : IPersistableEntity, new()
        {
            try
            {
                var entity = container.ReadItemAsync<object>(id, new PartitionKey(id)).GetAwaiter().GetResult();
                if (entity != null)
                {
                    return ((JObject) entity.Resource).FromContainerEntity<TEntity>();
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
        public static TEntity FromContainerEntity<TEntity>(this JObject containerEntity)
            where TEntity : IPersistableEntity, new()
        {
            return (TEntity) containerEntity.FromContainerEntity(new TEntity().GetType());
        }

        public static IPersistableEntity FromContainerEntity(this JObject containerEntity, Type entityType)
        {
            var entityPropertyTypeInfo = entityType.GetProperties();
            var containerEntityProperties = containerEntity
                .ToObject<Dictionary<string, object>>();

            ComplexTypesFromContainerEntity(containerEntityProperties, entityPropertyTypeInfo);

            if (!containerEntityProperties.ContainsKey(nameof(IPersistableEntity.Id)))
            {
                var entityId = containerEntityProperties[AzureCosmosSqlApiRepository.IdentifierPropertyName];
                containerEntityProperties.Add(nameof(IPersistableEntity.Id), entityId);
            }
            if (containerEntityProperties.ContainsKey(AzureCosmosSqlApiRepository.IdentifierPropertyName))
            {
                containerEntityProperties.Remove(AzureCosmosSqlApiRepository.IdentifierPropertyName);
            }
            foreach (var reservedPropertyName in AzureCosmosSqlApiRepository.CosmosReservedPropertyNames)
            {
                containerEntityProperties.Remove(reservedPropertyName);
            }

            var entity = entityType.CreateInstance<IPersistableEntity>();
            entity.Rehydrate(containerEntityProperties);
            return entity;
        }

        private static void ComplexTypesFromContainerEntity(
            Dictionary<string, object> containerEntityProperties, PropertyInfo[] entityPropertyTypeInfo)
        {
            var convertedEntityProperties = new Dictionary<string, object>();
            foreach (var containerEntityProperty in containerEntityProperties)
            {
                var entityPropertyType = entityPropertyTypeInfo.FirstOrDefault(ep =>
                    StringExtensions.EqualsIgnoreCase(ep.Name, containerEntityProperty.Key));
                if (entityPropertyType != null)
                {
                    var propertyType = entityPropertyType.PropertyType;
                    if (propertyType.IsComplexStorageType())
                    {
                        var json = (string) containerEntityProperty.Value;
                        if (json.HasValue())
                        {
                            convertedEntityProperties.Add(containerEntityProperty.Key,
                                JsonConvert.DeserializeObject(json, propertyType));
                        }
                    }
                }
            }

            foreach (var convertedProperty in convertedEntityProperties)
            {
                containerEntityProperties[convertedProperty.Key] = convertedProperty.Value;
            }
        }

        public static dynamic ToContainerEntity<TEntity>(this TEntity entity) where TEntity : IPersistableEntity
        {
            var containerEntity = new ExpandoObject();
            var containerEntityProperties = (IDictionary<string, object>) containerEntity;
            var entityProperties = entity.Dehydrate();
            foreach (var pair in entityProperties)
            {
                var value = pair.Value;
                if (value != null
                    && value.GetType().IsComplexStorageType())
                {
                    value = value.ToJson();
                }

                if (pair.Value is DateTime dateTime)
                {
                    if (dateTime == DateTime.MinValue)
                    {
                        value = DateTime.MinValue.ToUniversalTime();
                    }
                }

                containerEntityProperties.Add(pair.Key, value);
            }

            containerEntityProperties.Remove(nameof(IPersistableEntity.EntityName));
            containerEntityProperties.Add(AzureCosmosSqlApiRepository.IdentifierPropertyName, entity.Id);

            return containerEntity;
        }

        public static int Count(this FeedIterator<int> iterator)
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

    public static class AzureCosmosSqlApiWhereExtensions
    {
        public static string ToAzureCosmosSqlApiWhereClause<TEntity>(this QueryClause<TEntity> query,
            string containerName) where TEntity : INamedEntity, new()
        {
            var builder = new StringBuilder();
            builder.Append(@"SELECT ");

            if (query.PrimaryEntity.Selects.Any())
            {
                builder.Append($"{AzureCosmosSqlApiRepository.ContainerAlias}.{nameof(IPersistableEntity.Id)}");
                foreach (var select in query.PrimaryEntity.Selects)
                {
                    builder.Append($", {AzureCosmosSqlApiRepository.ContainerAlias}.{select.FieldName}");
                }
            }
            else
            {
                builder.Append(@"*");
            }

            builder.Append($" FROM {containerName} {AzureCosmosSqlApiRepository.ContainerAlias}");

            var wheres = query.Wheres.ToAzureCosmosSqlApiWhereClause();
            if (wheres.HasValue())
            {
                builder.Append($" WHERE {wheres}");
            }

            return builder.ToString();
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
                    $"{where.Operator.ToAzureCosmosSqlApiWhereClause()}{condition.ToAzureCosmosSqlApiWhereClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToAzureCosmosSqlApiWhereClause()}");
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

        private static string ToAzureCosmosSqlApiWhereClause(this LogicalOperator op)
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

        private static string ToAzureCosmosSqlApiWhereClause(this ConditionOperator op)
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

        private static string ToAzureCosmosSqlApiWhereClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName;
            var conditionOperator = condition.Operator.ToAzureCosmosSqlApiWhereClause();

            var value = condition.Value;
            switch (value)
            {
                case string text:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{text}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{dateTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}'"
                        : $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{dateTime:yyyy-MM-ddTHH:mm:ssZ}'";

                case DateTimeOffset dateTimeOffset:
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{dateTimeOffset:O}'";

                case bool _:
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} {value.ToString().ToLowerInvariant()}";

                case double _:
                case int _:
                case long _:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} {value}";

                case byte[] bytes:
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{Convert.ToBase64String(bytes)}'";

                case Guid guid:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{guid:D}'";

                case null:
                    return $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} null";

                default:
                    //Complex type?
                    return
                        $"{AzureCosmosSqlApiRepository.ContainerAlias}.{fieldName} {conditionOperator} '{value.ToEscapedString()}'";
            }
        }

        private static string ToEscapedString(this object value)
        {
            var escapedJson = value
                .ToString()
                .Replace("\"", "\\\"");

            return escapedJson;
        }
    }
}