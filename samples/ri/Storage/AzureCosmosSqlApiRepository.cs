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

namespace Storage
{
    public class AzureCosmosSqlApiRepository : IAzureCosmosRepository
    {
        private const string DefaultPartitionKeyPath = "/id";

        private const int DefaultThroughput = 400;
        private readonly string connectionString;
        private readonly string databaseName;
        private readonly IIdentifierFactory idFactory;
        private CosmosClient client;
        private Container container;
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

        public string Add<TEntity>(string containerName, TEntity entity) where TEntity : IKeyedEntity, new()
        {
            EnsureContainer(containerName);

            var id = this.idFactory.Create(entity);
            entity.Id = id;

            this.container.CreateItemAsync<dynamic>(entity.ToContainerEntity()).GetAwaiter().GetResult();

            return id;
        }

        public void Remove<TEntity>(string containerName, string id) where TEntity : IKeyedEntity, new()
        {
            EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(this.container, id);
            if (containerEntity != null)
            {
                this.container.DeleteItemAsync<object>(id, new PartitionKey(id)).GetAwaiter().GetResult();
            }
        }

        public TEntity Get<TEntity>(string containerName, string id) where TEntity : IKeyedEntity, new()
        {
            EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(this.container, id);

            return containerEntity;
        }

        public void Update<TEntity>(string containerName, string entityId, TEntity entity)
            where TEntity : IKeyedEntity, new()
        {
            EnsureContainer(containerName);

            var containerEntity = RetrieveContainerEntitySafe<TEntity>(this.container, entityId);
            if (containerEntity != null)
            {
                this.container.UpsertItemAsync(entity.ToContainerEntity());
            }
        }

        public long Count(string containerName)
        {
            EnsureContainer(containerName);

            try
            {
                var query = new QueryDefinition(@"SELECT VALUE COUNT(1) FROM c");
                var count = this.container.GetItemQueryIterator<int>(query);
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
            where TEntity : IKeyedEntity, new()
        {
            EnsureContainer(containerName);
            var containerQuery = new QueryDefinition(query.Wheres.ToAzureCosmosSqlApiWhereClause(containerName));
            try
            {
                var results = this.container.GetItemQueryIterator<object>(containerQuery)
                    .ToList();
                return results.ConvertAll(r => r.FromContainerEntity<TEntity>());
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
            EnsureContainer(containerName);
            this.container.DeleteContainerAsync().GetAwaiter().GetResult();
            this.container = null;
        }

        public void Dispose()
        {
            // No need to do anything here. IDisposable is used as a marker interface
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

        private void EnsureContainer(string containerName)
        {
            EnsureConnected();

            if (this.container == null)
            {
                this.container = this.client.GetDatabase(this.databaseName)
                    .CreateContainerIfNotExistsAsync(containerName, DefaultPartitionKeyPath, DefaultThroughput)
                    .GetAwaiter()
                    .GetResult().Container;
            }
        }

        private static TEntity RetrieveContainerEntitySafe<TEntity>(Container container, string id)
            where TEntity : IKeyedEntity, new()
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
            where TEntity : IKeyedEntity, new()
        {
            var entityPropertyTypeInfo = new TEntity().GetType().GetProperties();
            var containerEntityProperties = containerEntity.ToObject<Dictionary<string, object>>();

            var convertedEntityProperties = new Dictionary<string, object>();
            ComplexTypesFromContainerEntity(containerEntityProperties, entityPropertyTypeInfo,
                convertedEntityProperties);

            var entity = containerEntityProperties.FromObjectDictionary<TEntity>();

            return entity;
        }

        private static void ComplexTypesFromContainerEntity(
            Dictionary<string, object> containerEntityProperties, PropertyInfo[] entityPropertyTypeInfo,
            Dictionary<string, object> convertedEntityProperties)
        {
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

        public static dynamic ToContainerEntity<TEntity>(this TEntity entity) where TEntity : IKeyedEntity
        {
            var containerEntity = new ExpandoObject();
            var containerEntityProperties = (IDictionary<string, object>) containerEntity;
            var entityProperties = entity.ToObjectDictionary();
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

            containerEntityProperties.Remove(nameof(IKeyedEntity.EntityName));
            containerEntityProperties.Add("id", entityProperties[nameof(IKeyedEntity.Id)]);

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

    internal static class AzureCosmosSqlApiWhereExtensions
    {
        public static string ToAzureCosmosSqlApiWhereClause(this IReadOnlyList<WhereExpression> wheres,
            string containerName)
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT * FROM {containerName} t");
            if (wheres.Any())
            {
                builder.Append(" WHERE ");
                foreach (var where in wheres)
                {
                    builder.Append(where.ToAzureCosmosSqlApiWhereClause());
                }
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
                    return $"t.{fieldName} {conditionOperator} '{text}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? $"t.{fieldName} {conditionOperator} '{dateTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}'"
                        : $"t.{fieldName} {conditionOperator} '{dateTime:yyyy-MM-ddTHH:mm:ssZ}'";

                case DateTimeOffset dateTimeOffset:
                    return $"t.{fieldName} {conditionOperator} '{dateTimeOffset:O}'";

                case bool _:
                    return $"t.{fieldName} {conditionOperator} {value.ToString().ToLowerInvariant()}";

                case double _:
                case int _:
                case long _:
                    return $"t.{fieldName} {conditionOperator} {value}";

                case byte[] bytes:
                    return $"t.{fieldName} {conditionOperator} '{Convert.ToBase64String(bytes)}'";

                case Guid guid:
                    return $"t.{fieldName} {conditionOperator} '{guid:D}'";

                case null:
                    return $"t.{fieldName} {conditionOperator} null";

                default:
                    //Complex type?
                    return $"t.{fieldName} {conditionOperator} '{value.ToEscapedString()}'";
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