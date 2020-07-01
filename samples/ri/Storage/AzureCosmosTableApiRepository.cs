using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Protocol;
using Newtonsoft.Json;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public class AzureCosmosTableApiRepository : IAzureCosmosRepository
    {
        internal const string NullValue = "null";
        internal const string DefaultPartitionKey = "default";
        private readonly string connectionString;
        private readonly IIdentifierFactory idFactory;
        private readonly Dictionary<string, bool> tableExistenceChecks = new Dictionary<string, bool>();
        private CloudTableClient client;

        public AzureCosmosTableApiRepository(string connectionString, IIdentifierFactory idFactory)
        {
            Guard.AgainstNullOrEmpty(() => connectionString, connectionString);
            Guard.AgainstNull(() => idFactory, idFactory);
            this.connectionString = connectionString;
            this.idFactory = idFactory;
        }

        public string Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new()
        {
            var table = EnsureTable(containerName);

            var id = this.idFactory.Create(entity);
            entity.Id = id;

            table.Execute(TableOperation.Insert(entity.ToTableEntity()));

            return id;
        }

        public void Remove<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            var table = EnsureTable(containerName);

            var tableEntity = RetrieveTableEntitySafe(table, id);
            if (tableEntity != null)
            {
                table.Execute(TableOperation.Delete(tableEntity));
            }
        }

        public TEntity Get<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
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
            where TEntity : IPersistableEntity, new()
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

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new()
        {
            var table = EnsureTable(containerName);
            var filter = query.Wheres.ToAzureCosmosTableApiWhereClause();
            var tableQuery = new TableQuery<DynamicTableEntity>().Where(filter);

            if (query.PrimaryEntity.Selects.Any())
            {
                tableQuery.SelectColumns = query.PrimaryEntity.Selects
                    .Select(sel => sel.FieldName)
                    .ToList();
            }

            var primaryResults = table.ExecuteQuery(tableQuery)
                .ToList();

            var joinedTables = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.Name, je => new
                {
                    Collection = QueryJoiningTable(je,
                        primaryResults.Select(e => e.Properties[je.Join.Left.JoinedFieldName])),
                    JoinedEntity = je
                });

            var primaryEntities = primaryResults
                .ConvertAll(r => r.FromTableEntity<TEntity>());

            if (joinedTables.Any())
            {
                foreach (var joinedTable in joinedTables)
                {
                    var joinedEntity = joinedTable.Value.JoinedEntity;
                    var join = joinedEntity.Join;
                    var leftEntities = primaryEntities.ToDictionary(e => e.Id, e => e.Dehydrate());
                    var rightEntities = joinedTable.Value.Collection.ToDictionary(e => e.RowKey,
                        e => e.FromTableEntity(join.Right.EntityType).Dehydrate());

                    primaryEntities = join
                        .JoinResults<TEntity>(leftEntities, rightEntities,
                            joinedEntity.Selects.ProjectSelectedJoinedProperties());
                }
            }

            return primaryEntities.CherryPickSelectedProperties(query);
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

        private List<DynamicTableEntity> QueryJoiningTable(QueriedEntity<INamedEntity> joinedEntity,
            IEnumerable<EntityProperty> propertyValues)
        {
            var tableName = joinedEntity.Name;
            var table = EnsureTable(tableName);

            //HACK: pretty limited way to query lots of entities by individual Id
            var counter = 0;
            var query = propertyValues.Select(propertyValue => new WhereExpression
            {
                Condition = new WhereCondition
                {
                    FieldName = joinedEntity.Join.Right.JoinedFieldName,
                    Operator = ConditionOperator.EqualTo,
                    Value = propertyValue.PropertyAsObject
                },
                Operator = counter++ == 0
                    ? LogicalOperator.None
                    : LogicalOperator.Or
            }).ToAzureCosmosTableApiWhereClause();

            var selectedPropertyNames = joinedEntity.Selects
                .Where(sel => sel.JoinedFieldName.HasValue())
                .Select(j => j.JoinedFieldName)
                .Concat(new[] {joinedEntity.Join.Right.JoinedFieldName, nameof(IPersistableEntity.Id)})
                .ToList();

            var filter = $"({TableConstants.PartitionKey} eq '{DefaultPartitionKey}') and ({query})";
            var tableQuery = new TableQuery<DynamicTableEntity>().Where(filter);
            tableQuery.SelectColumns = selectedPropertyNames;

            return table.ExecuteQuery(tableQuery)
                .ToList();
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

            if (IsTableExistenceCheckPerformed(containerName))
            {
                return table;
            }

            if (!table.Exists())
            {
                table.Create();
            }

            return table;
        }

        private bool IsTableExistenceCheckPerformed(string containerName)
        {
            if (!this.tableExistenceChecks.ContainsKey(containerName))
            {
                this.tableExistenceChecks.Add(containerName, false);
            }

            if (this.tableExistenceChecks[containerName])
            {
                return true;
            }

            this.tableExistenceChecks[containerName] = true;
            return false;
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

    internal static class AzureCosmosTableApiEntityExtensions
    {
        public static TEntity FromTableEntity<TEntity>(this DynamicTableEntity tableEntity)
            where TEntity : IPersistableEntity, new()
        {
            return (TEntity) tableEntity.FromTableEntity(new TEntity().GetType());
        }

        public static IPersistableEntity FromTableEntity(this DynamicTableEntity tableEntity, Type entityType)
        {
            var entityPropertyTypes = entityType.GetProperties();
            var propertyValues = tableEntity.Properties
                .Where(te =>
                    entityPropertyTypes.Any(prop => prop.Name.EqualsOrdinal(te.Key)) &&
                    te.Value.PropertyAsObject != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromTableEntityProperty(entityPropertyTypes
                        .First(prop => prop.Name.EqualsOrdinal(pair.Key)).PropertyType));

            var entity = entityType.CreateInstance<IPersistableEntity>();
            entity.Rehydrate(propertyValues);
            entity.Id = tableEntity.RowKey;
            return entity;
        }

        public static DynamicTableEntity ToTableEntity<TEntity>(this TEntity entity) where TEntity : IPersistableEntity
        {
            bool IsNotExcluded(string propertyName)
            {
                var excludedPropertyNames = new[] {nameof(IPersistableEntity.Id), nameof(INamedEntity.EntityName)};
                return !excludedPropertyNames.Contains(propertyName);
            }

            var entityProperties = entity.Dehydrate()
                .Where(pair => IsNotExcluded(pair.Key));
            var tableEntity = new DynamicTableEntity(AzureCosmosTableApiRepository.DefaultPartitionKey, entity.Id)
            {
                Properties = entityProperties.ToTableEntityProperties()
            };

            return tableEntity;
        }

        private static Dictionary<string, EntityProperty> ToTableEntityProperties(
            this IEnumerable<KeyValuePair<string, object>> entityProperties)
        {
            EntityProperty ToEntityProperty(object property)
            {
                switch (property)
                {
                    case string text:
                        return EntityProperty.GeneratePropertyForString(text);
                    case DateTime dateTime:
                        return EntityProperty.GeneratePropertyForDateTimeOffset(
                            ToTableEntityDateTimeOffsetProperty(dateTime));
                    case DateTimeOffset dateTimeOffset:
                        return EntityProperty.GeneratePropertyForDateTimeOffset(
                            ToTableEntityDateTimeOffsetProperty(dateTimeOffset));
                    case bool boolean:
                        return EntityProperty.GeneratePropertyForBool(boolean);
                    case int int32:
                        return EntityProperty.GeneratePropertyForInt(int32);
                    case long int64:
                        return EntityProperty.GeneratePropertyForLong(int64);
                    case double @double:
                        return EntityProperty.GeneratePropertyForDouble(@double);
                    case Guid guid:
                        return EntityProperty.GeneratePropertyForGuid(guid);
                    case byte[] bytes:
                        return EntityProperty.GeneratePropertyForByteArray(bytes);
                    case null:
                        return EntityProperty.CreateEntityPropertyFromObject(AzureCosmosTableApiRepository.NullValue);
                    default:
                        var value = property.ToJson();
                        return EntityProperty.GeneratePropertyForString(value);
                }
            }

            return entityProperties
                .ToDictionary(pair => pair.Key,
                    pair => ToEntityProperty(pair.Value));
        }

        private static object FromTableEntityProperty(this EntityProperty property, Type targetEntityType)
        {
            var value = property.PropertyAsObject;
            switch (value)
            {
                case string text:
                    if (text.EqualsOrdinal(AzureCosmosTableApiRepository.NullValue))
                    {
                        return null;
                    }

                    if (targetEntityType.IsComplexStorageType())
                    {
                        if (text.StartsWith("{") && text.EndsWith("}"))
                        {
                            return JsonConvert.DeserializeObject(text, targetEntityType);
                        }

                        return null;
                    }

                    return text;

                case DateTime _:
                case DateTimeOffset _:
                case bool _:
                case int _:
                case long _:
                case double _:
                case Guid _:
                case byte[] _:
                    return value;

                case null:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(property));
            }
        }

        private static DateTimeOffset? ToTableEntityDateTimeOffsetProperty(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.HasValue()
                    ? dateTime.Kind == DateTimeKind.Utc
                        ? new DateTimeOffset(dateTime.ToUniversalTime(), TimeSpan.Zero)
                        : new DateTimeOffset(dateTime.ToLocalTime())
                    : DateTimeOffset.MinValue;
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.DateTime.HasValue()
                    ? dateTimeOffset
                    : DateTimeOffset.MinValue;
            }

            return null;
        }
    }

    public static class AzureCosmosTableApiWhereExtensions
    {
        public static string ToAzureCosmosTableApiWhereClause(this IEnumerable<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToAzureCosmosTableApiWhereClause());
            }

            return builder.ToString();
        }

        private static string ToAzureCosmosTableApiWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;
                return
                    $"{where.Operator.ToAzureCosmosTableApiWhereClause()}{condition.ToAzureCosmosTableApiWhereClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToAzureCosmosTableApiWhereClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToAzureCosmosTableApiWhereClause()}");
                }

                builder.Append(")");
                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToAzureCosmosTableApiWhereClause(this LogicalOperator op)
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

        private static string ToAzureCosmosTableApiWhereClause(this ConditionOperator op)
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

        internal static string ToAzureCosmosTableApiWhereClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName.EqualsOrdinal(nameof(IPersistableEntity.Id))
                ? TableConstants.RowKey
                : condition.FieldName;
            var conditionOperator = condition.Operator.ToAzureCosmosTableApiWhereClause();

            var value = condition.Value;
            switch (value)
            {
                case string text:
                    return TableQuery.GenerateFilterCondition(fieldName, conditionOperator, text);
                case DateTime dateTime:
                    return TableQuery.GenerateFilterConditionForDate(fieldName, conditionOperator,
                        new DateTimeOffset(dateTime.ToUniversalTime(), TimeSpan.Zero));
                case DateTimeOffset dateTimeOffset:
                    return TableQuery.GenerateFilterConditionForDate(fieldName, conditionOperator, dateTimeOffset);
                case bool boolean:
                    return TableQuery.GenerateFilterConditionForBool(fieldName, conditionOperator, boolean);
                case double @double:
                    return TableQuery.GenerateFilterConditionForDouble(fieldName, conditionOperator, @double);
                case int int32:
                    return TableQuery.GenerateFilterConditionForInt(fieldName, conditionOperator, int32);
                case long int64:
                    return TableQuery.GenerateFilterConditionForLong(fieldName, conditionOperator, int64);
                case byte[] bytes:
                    return TableQuery.GenerateFilterConditionForBinary(fieldName, conditionOperator, bytes);
                case Guid guid:
                    return TableQuery.GenerateFilterConditionForGuid(fieldName, conditionOperator, guid);
                case null:
                    return TableQuery.GenerateFilterCondition(fieldName, conditionOperator,
                        AzureCosmosTableApiRepository.NullValue);
                default:
                    //Complex type?
                    return TableQuery.GenerateFilterCondition(fieldName, conditionOperator, value.ToJson());
            }
        }
    }
}