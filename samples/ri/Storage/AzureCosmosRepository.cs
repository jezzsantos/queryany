using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Protocol;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public interface IAzureCosmosRepository : IDisposable
    {
        string Add<TEntity>(string containerName, TEntity entity) where TEntity : IKeyedEntity, new();
        void Remove(string containerName, string id);
        TEntity Get<TEntity>(string containerName, string id) where TEntity : IKeyedEntity, new();
        void Update<TEntity>(string containerName, string entityId, TEntity entity) where TEntity : IKeyedEntity, new();
        long Count(string containerName);
        List<TEntity> Query<TEntity>(string containerName, string query) where TEntity : IKeyedEntity, new();
        void DestroyAll(string containerName);
    }

    public class AzureCosmosRepository : IAzureCosmosRepository
    {
        internal const string DefaultPartitionKey = "default";
        private readonly string connectionString;
        private readonly IIdentifierFactory idFactory;
        private CloudTableClient client;
        private bool tableExistCheckPerformed;

        public AzureCosmosRepository(string connectionString, IIdentifierFactory idFactory)
        {
            Guard.AgainstNullOrEmpty(() => connectionString, connectionString);
            Guard.AgainstNull(() => idFactory, idFactory);
            this.connectionString = connectionString;
            this.idFactory = idFactory;
        }

        public string Add<TEntity>(string containerName, TEntity entity) where TEntity : IKeyedEntity, new()
        {
            var table = EnsureTable(containerName);

            var id = this.idFactory.Create(entity);
            entity.Id = id;

            table.Execute(TableOperation.Insert(entity.ToTableEntity()));

            return id;
        }

        public void Remove(string containerName, string id)
        {
            var table = EnsureTable(containerName);

            var tableEntity = RetrieveTableEntitySafe(table, id);
            if (tableEntity != null)
            {
                table.Execute(TableOperation.Delete(tableEntity));
            }
        }

        public TEntity Get<TEntity>(string containerName, string id) where TEntity : IKeyedEntity, new()
        {
            var table = EnsureTable(containerName);

            var tableEntity = RetrieveTableEntitySafe(table, id);
            if (tableEntity != null)
            {
                return tableEntity.FromTableEntity<TEntity>();
            }

            return default;
        }

        public void Update<TEntity>(string containerName, string entityId, TEntity entity)
            where TEntity : IKeyedEntity, new()
        {
            var table = EnsureTable(containerName);

            var tableEntity = RetrieveTableEntitySafe(table, entityId);
            if (tableEntity != null)
            {
                table.Execute(TableOperation.InsertOrReplace(entity.ToTableEntity()));
            }
        }

        public long Count(string containerName)
        {
            var table = EnsureTable(containerName);

            var query = new TableQuery()
                .Where(TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal,
                    DefaultPartitionKey))
                .Select(new List<string>
                    {TableConstants.PartitionKey, TableConstants.RowKey, TableConstants.Timestamp});
            var results = table.ExecuteQuery(query);

            return results.LongCount();
        }

        public List<TEntity> Query<TEntity>(string containerName, string query) where TEntity : IKeyedEntity, new()
        {
            var table = EnsureTable(containerName);

            var results = table.ExecuteQuery(new TableQuery<DynamicTableEntity>().Where(query));

            return results.ToList().ConvertAll(r => r.FromTableEntity<TEntity>());
        }

        public void DestroyAll(string containerName)
        {
            var table = EnsureTable(containerName);

            //HACK: deleting the entire table takes too long (unreliable in testing)
            var query = new TableQuery()
                .Where(TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal,
                    DefaultPartitionKey))
                .Select(new List<string>
                    {TableConstants.PartitionKey, TableConstants.RowKey, TableConstants.Timestamp});
            var results = table.ExecuteQuery(query);
            foreach (var result in results)
            {
                table.Execute(TableOperation.Delete(result));
            }
        }

        public void Dispose()
        {
            // No need to do anything here. IDisposable is used as a marker interface
        }

        private void EnsureConnected()
        {
            if (this.client != null)
            {
                return;
            }

            var account = CloudStorageAccount.Parse(this.connectionString);
            this.client = account.CreateCloudTableClient();
        }

        private CloudTable EnsureTable(string containerName)
        {
            EnsureConnected();
            var table = this.client.GetTableReference(containerName);

            if (this.tableExistCheckPerformed)
            {
                return table;
            }

            this.tableExistCheckPerformed = true;
            if (!table.Exists())
            {
                table.Create();
            }

            return table;
        }

        private static DynamicTableEntity RetrieveTableEntitySafe(CloudTable table, string id)
        {
            try
            {
                var entity = table.Execute(TableOperation.Retrieve<DynamicTableEntity>(DefaultPartitionKey, id));
                return entity.Result as DynamicTableEntity;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    internal static class EntityExtensions
    {
        public static TEntity FromTableEntity<TEntity>(this DynamicTableEntity tableEntity) where TEntity : IKeyedEntity
        {
            var properties = tableEntity.Properties
                .ToDictionary(pair => pair.Key, pair => pair.Value.PropertyAsObject);

            var entity = properties.FromObjectDictionary<TEntity>();
            entity.Id = tableEntity.RowKey;
            return entity;
        }

        public static DynamicTableEntity ToTableEntity<TEntity>(this TEntity entity) where TEntity : IKeyedEntity
        {
            var tableEntity = new DynamicTableEntity(AzureCosmosRepository.DefaultPartitionKey, entity.Id)
            {
                Properties = entity.ToEntityProperties()
            };

            return tableEntity;
        }

        private static Dictionary<string, EntityProperty> ToEntityProperties<TEntity>(this TEntity entity)
            where TEntity : IKeyedEntity
        {
            EntityProperty CreateEntityPropertyFromObject(KeyValuePair<string, object> property)
            {
                return property.Value is DateTime
                    ? EntityProperty.CreateEntityPropertyFromObject(RebaseDateTimeForStorage(property.Value))
                    : EntityProperty.CreateEntityPropertyFromObject(property.Value);
            }

            var excludedPropertyNames = new[] {nameof(IKeyedEntity.Id), nameof(INamedEntity.EntityName)};

            return entity.ToObjectDictionary()
                .Where(pair => !excludedPropertyNames.Contains(pair.Key))
                .ToDictionary(pair => pair.Key,
                    CreateEntityPropertyFromObject);
        }

        private static object RebaseDateTimeForStorage(object value)
        {
            var minAzureDateTime = TableConstants.MinDateTime.DateTime;

            if (!(value is DateTime dateTime))
            {
                return value;
            }

            return dateTime < minAzureDateTime
                ? minAzureDateTime
                : value;
        }
    }

    public static class AzureCosmosWhereExtensions
    {
        public static string ToAzureCosmosWhereClause(this IEnumerable<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToAzureCosmosWhereClause());
            }

            return builder.ToString();
        }

        private static string ToAzureCosmosWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;
                return
                    $"{where.Operator.ToAzureCosmosWhereClause()}{condition.ToAzureCosmosWhereClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToAzureCosmosWhereClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToAzureCosmosWhereClause()}");
                }

                builder.Append(")");
                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToAzureCosmosWhereClause(this LogicalOperator op)
        {
            switch (op)
            {
                case LogicalOperator.None:
                    return string.Empty;
                case LogicalOperator.And:
                    return " and ";
                case LogicalOperator.Or:
                    return " or ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToAzureCosmosWhereClause(this ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.EqualTo:
                    return "eq";
                case ConditionOperator.GreaterThan:
                    return "gt";
                case ConditionOperator.GreaterThanEqualTo:
                    return "ge";
                case ConditionOperator.LessThan:
                    return "lt";
                case ConditionOperator.LessThanEqualTo:
                    return "le";
                case ConditionOperator.NotEqualTo:
                    return "ne";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToAzureCosmosWhereClause(this WhereCondition condition)
        {
            if (condition.Value is DateTime dateTime)
            {
                if (!dateTime.HasValue())
                {
                    dateTime = TableConstants.MinDateTime.DateTime;
                }

                var dateValue = new DateTimeOffset(dateTime);
                return TableQuery.GenerateFilterConditionForDate(condition.FieldName,
                    condition.Operator.ToAzureCosmosWhereClause(), dateValue);
            }

            if (condition.Value is bool givenBool)
            {
                // Because boolean values have been converted to strings in storage
                var boolValue = givenBool.ToLower();
                return TableQuery.GenerateFilterCondition(condition.FieldName,
                    condition.Operator.ToAzureCosmosWhereClause(), boolValue);
            }

            var stringValue = condition.Value.ToString();
            return TableQuery.GenerateFilterCondition(condition.FieldName,
                condition.Operator.ToAzureCosmosWhereClause(), stringValue);
        }
    }
}