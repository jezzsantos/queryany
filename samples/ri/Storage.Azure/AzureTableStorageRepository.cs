using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Protocol;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Configuration;

namespace Storage.Azure
{
    // ReSharper disable once InconsistentNaming
    public class AzureTableStorageRepository : IRepository
    {
        internal const string NullValue = "null";
        private readonly string connectionString;
        private readonly TableStorageApiOptions options;
        private readonly Dictionary<string, bool> tableExistenceChecks = new Dictionary<string, bool>();

        private CloudTableClient client;

        public AzureTableStorageRepository(string connectionString) : this(
            connectionString, TableStorageApiOptions.AzureTableStorage)
        {
        }

        protected AzureTableStorageRepository(string connectionString,
            TableStorageApiOptions options)
        {
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            options.GuardAgainstNull(nameof(options));
            this.connectionString = connectionString;
            this.options = options;
        }

        public int MaxQueryResults => TableConstants.TableServiceMaxResults;

        public CommandEntity Add(string containerName, CommandEntity entity)

        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            entity.GuardAgainstNull(nameof(entity));

            var table = EnsureTable(containerName);

            SafeExecute(table, () => { table.Execute(TableOperation.Insert(entity.ToTableEntity(this.options))); });

            return Retrieve(containerName, entity.Id, entity.Metadata);
        }

        public void Remove(string containerName, string id)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));

            var table = EnsureTable(containerName);

            var tableEntity = RetrieveTableEntitySafe(table, id);
            if (tableEntity != null)
            {
                SafeExecute(table, () => { table.Execute(TableOperation.Delete(tableEntity)); });
            }
        }

        public CommandEntity Retrieve(string containerName, string id, RepositoryEntityMetadata metadata)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));
            metadata.GuardAgainstNull(nameof(metadata));

            var table = EnsureTable(containerName);

            var tableEntity = RetrieveTableEntitySafe(table, id);

            return tableEntity != null
                ? CommandEntity.FromCommandEntity(tableEntity.FromTableEntity(metadata, this.options), metadata)
                : default;
        }

        public CommandEntity Replace(string containerName, string id, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNull(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var table = EnsureTable(containerName);

            var result = SafeExecute(table,
                    () => table.Execute(TableOperation.InsertOrReplace(entity.ToTableEntity(this.options))))
                .Result as DynamicTableEntity;

            return CommandEntity.FromCommandEntity(result.FromTableEntity(entity.Metadata, this.options), entity);
        }

        public long Count(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var table = EnsureTable(containerName);

            var query = new TableQuery()
                .Where(TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal,
                    this.options.DefaultPartitionKey))
                .Select(new List<string>
                    {TableConstants.PartitionKey, TableConstants.RowKey, TableConstants.Timestamp});
            var results = SafeExecute(table, () => table.ExecuteQuery(query));

            return results.LongCount();
        }

        public List<QueryEntity> Query<TQueryableEntity>(string containerName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            if (query == null || query.Options.IsEmpty)
            {
                return new List<QueryEntity>();
            }

            var table = EnsureTable(containerName);

            var primaryEntities = QueryPrimaryEntities(table, query, metadata);

            var joinedTables = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.EntityName, je => new
                {
                    Collection = QueryJoiningTable(je,
                        primaryEntities.Select(e =>
                            CommandEntity.FromProperties(e.Properties, metadata).ToTableEntity(this.options)[
                                je.Join.Left.JoinedFieldName])),
                    JoinedEntity = je
                });

            if (joinedTables.Any())
            {
                foreach (var joinedTable in joinedTables)
                {
                    var joinedEntity = joinedTable.Value.JoinedEntity;
                    var join = joinedEntity.Join;
                    var leftEntities = primaryEntities.ToDictionary(e => e.Id.ToString(), e => e.Properties);
                    var rightEntities = joinedTable.Value.Collection
                        .ToDictionary(e => e.RowKey,
                            e => e.FromTableEntity(RepositoryEntityMetadata.FromType(join.Right.EntityType),
                                this.options));

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

            var table = EnsureTable(containerName);

            // HACK: deleting the entire table takes far too long (only reliable in testing)
            List<DynamicTableEntity> GetRemaining()
            {
                var query = new TableQuery()
                    .Where(TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal,
                        this.options.DefaultPartitionKey))
                    .Select(new List<string>
                        {TableConstants.PartitionKey, TableConstants.RowKey, TableConstants.Timestamp});

                return table.ExecuteQuery(query).ToList();
            }

            void DeleteInBatches(IEnumerable<DynamicTableEntity> entities)
            {
                void Command(TableBatchOperation batch)
                {
                    try
                    {
                        table.ExecuteBatch(batch);
                    }
                    catch (Exception)
                    {
                        // Ignore delete
                    }
                }

                var batches = new Dictionary<string, TableBatchOperation>();
                foreach (var entity in entities)
                {
                    if (!entity.ETag.HasValue())
                    {
                        entity.ETag = "*";
                    }

                    var partitionKey = entity.PartitionKey;
                    if (batches.TryGetValue(partitionKey, out var batch) == false)
                    {
                        batch = new TableBatchOperation();
                        batches[partitionKey] = batch;
                    }

                    batch.Add(TableOperation.Delete(entity));

                    if (batch.Count != TableConstants.TableServiceBatchMaximumOperations)
                    {
                        continue;
                    }

                    SafeExecute(table, () => Command(batch));
                    batches[partitionKey] = new TableBatchOperation();
                }

                foreach (var batch in batches.Values)
                {
                    if (batch.Count > 0)
                    {
                        SafeExecute(table, () => Command(batch));
                    }
                }
            }

            var remaining = GetRemaining();
            while (remaining.Any())
            {
                DeleteInBatches(remaining);
                remaining = GetRemaining();
            }
        }

        public static AzureTableStorageRepository FromSettings(IAppSettings settings)
        {
            settings.GuardAgainstNull(nameof(settings));

            var accountKey = settings.GetString("AzureTableStorageAccountKey");
            var hostName = settings.GetString("AzureTableStorageHostName");
            var localEmulatorConnectionString = accountKey.HasValue()
                ? $"DefaultEndpointsProtocol=https;AccountName={hostName};AccountKey={accountKey};EndpointSuffix=core.windows.net"
                : "UseDevelopmentStorage=true";
            return new AzureTableStorageRepository(localEmulatorConnectionString);
        }

        private List<QueryEntity> QueryPrimaryEntities<TQueryableEntity>(CloudTable table,
            QueryClause<TQueryableEntity> query, RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            var filter = query.Wheres.ToAzureTableStorageWhereClause();
            var tableQuery = new TableQuery<DynamicTableEntity>()
                .Where(filter);

            if (query.PrimaryEntity.Selects.Any())
            {
                tableQuery.SelectColumns = query.PrimaryEntity.Selects
                    .Select(sel => sel.FieldName)
                    .Concat(new[] {query.GetDefaultOrdering()})
                    .ToList();
            }

            var take = query.GetDefaultTake(this);
            if (take == 0)
            {
                return new List<QueryEntity>();
            }

            // HACK: AzureTableStorage does not support Skip, nor OrderBy, nor Distinct
            // HACK: so we have to fetch all data and do Skip, OrderBy in memory
            return SafeExecute(table, () => table.ExecuteQuery(tableQuery))
                .ToDictionary(e => e.RowKey, e => e.FromTableEntity(metadata, this.options))
                .AsQueryable()
                .OrderBy(query.ToDynamicLinqOrderByClause())
                .Skip(query.GetDefaultSkip())
                .Take(take)
                .Select(pe => QueryEntity.FromProperties(pe.Value, metadata))
                .ToList();
        }

        private List<DynamicTableEntity> QueryJoiningTable(QueriedEntity joinedEntity,
            IEnumerable<EntityProperty> propertyValues)
        {
            var tableName = joinedEntity.EntityName;
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
            }).ToAzureTableStorageWhereClause();

            var selectedPropertyNames = joinedEntity.Selects
                .Where(sel => sel.JoinedFieldName.HasValue())
                .Select(j => j.JoinedFieldName)
                .Concat(new[] {joinedEntity.Join.Right.JoinedFieldName, nameof(QueryEntity.Id)})
                .ToList();

            var filter = $"({TableConstants.PartitionKey} eq '{this.options.DefaultPartitionKey}') and ({query})";
            var tableQuery = new TableQuery<DynamicTableEntity>().Where(filter);
            tableQuery.SelectColumns = selectedPropertyNames;

            return SafeExecute(table, () => table.ExecuteQuery(tableQuery))
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

        private DynamicTableEntity RetrieveTableEntitySafe(CloudTable table, string id)
        {
            try
            {
                var entity = SafeExecute(table,
                    () => table.Execute(
                        TableOperation.Retrieve<DynamicTableEntity>(this.options.DefaultPartitionKey, id)));

                return entity.Result as DynamicTableEntity;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void SafeExecute(CloudTable table, Action command)
        {
            SafeExecute(table, () =>
            {
                command();

                return true;
            });
        }

        private static TReturn SafeExecute<TReturn>(CloudTable table, Func<TReturn> command)
        {
            try
            {
                return command();
            }
            catch (StorageException ex)
            {
                if (ex.IsTableNotFoundException())
                {
                    try
                    {
                        table.CreateIfNotExists();

                        return command();
                    }
                    catch (StorageException ex2)
                    {
                        throw ex2.ToDetailedException();
                    }
                }

                if (ex.IsEntityNotFoundException())
                {
                    throw new ResourceNotFoundException(ex.Message, ex);
                }

                throw ex.ToDetailedException();
            }
        }

        public class TableStorageApiOptions
        {
            // ReSharper disable once InconsistentNaming
            public static readonly TableStorageApiOptions AzureTableStorage = new TableStorageApiOptions
                {DefaultPartitionKey = string.Empty, MinimumAllowableUtcDateTime = TableConstants.MinDateTime};

            // ReSharper disable once InconsistentNaming
            public static readonly TableStorageApiOptions AzureCosmosDbStorage = new TableStorageApiOptions
                {DefaultPartitionKey = "default", MinimumAllowableUtcDateTime = DateTimeOffset.MinValue};

            public string DefaultPartitionKey { get; private set; }

            public DateTimeOffset MinimumAllowableUtcDateTime { get; private set; }
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static class AzureTableStorageEntityExtensions
    {
        public static DynamicTableEntity ToTableEntity(this CommandEntity entity,
            AzureTableStorageRepository.TableStorageApiOptions options)
        {
            bool IsNotExcluded(string propertyName)
            {
                var excludedPropertyNames = new[] {nameof(CommandEntity.Id)};

                return !excludedPropertyNames.Contains(propertyName);
            }

            var entityProperties = entity.Properties
                .Where(pair => IsNotExcluded(pair.Key));
            var tableEntity = new DynamicTableEntity(options.DefaultPartitionKey, entity.Id)
            {
                Properties = entityProperties.ToTableEntityProperties(entity.Metadata, options)
            };

            tableEntity.Properties[nameof(CommandEntity.LastPersistedAtUtc)].DateTime = DateTime.UtcNow;

            return tableEntity;
        }

        private static Dictionary<string, EntityProperty> ToTableEntityProperties(
            this IEnumerable<KeyValuePair<string, object>> entityProperties,
            RepositoryEntityMetadata metadata,
            AzureTableStorageRepository.TableStorageApiOptions options)
        {
            return entityProperties
                .ToDictionary(pair => pair.Key,
                    pair => ToTableEntityProperty(pair.Value, metadata.GetPropertyType(pair.Key), options));
        }

        private static EntityProperty ToTableEntityProperty(object property, Type targetPropertyType,
            AzureTableStorageRepository.TableStorageApiOptions options)
        {
            switch (targetPropertyType)
            {
                case Type _ when targetPropertyType == typeof(DateTime) || targetPropertyType == typeof(DateTime?):
                {
                    DateTimeOffset? dateTimeOffset = null;
                    var dateTime = (DateTime?) property;
                    if (dateTime.HasValue)
                    {
                        dateTimeOffset = dateTime.Value.HasValue()
                            ? dateTime.Value.Kind == DateTimeKind.Utc
                                ? new DateTimeOffset(dateTime.Value.ToUniversalTime(), TimeSpan.Zero)
                                : new DateTimeOffset(dateTime.Value.ToLocalTime())
                            : options.MinimumAllowableUtcDateTime;
                    }

                    return EntityProperty.GeneratePropertyForDateTimeOffset(dateTimeOffset);
                }

                case Type _ when targetPropertyType == typeof(DateTimeOffset) ||
                                 targetPropertyType == typeof(DateTimeOffset?):
                {
                    var dateTimeOffset = (DateTimeOffset?) property;
                    if (dateTimeOffset.HasValue)
                    {
                        dateTimeOffset = dateTimeOffset.Value.DateTime.HasValue()
                            ? dateTimeOffset
                            : options.MinimumAllowableUtcDateTime;
                    }

                    return EntityProperty.GeneratePropertyForDateTimeOffset(dateTimeOffset);
                }

                case Type _ when targetPropertyType == typeof(bool) || targetPropertyType == typeof(bool?):
                    return EntityProperty.GeneratePropertyForBool((bool?) property);

                case Type _ when targetPropertyType == typeof(int) || targetPropertyType == typeof(int?):
                    return EntityProperty.GeneratePropertyForInt((int?) property);

                case Type _ when targetPropertyType == typeof(long) || targetPropertyType == typeof(long?):
                    return EntityProperty.GeneratePropertyForLong((long?) property);

                case Type _ when targetPropertyType == typeof(double) || targetPropertyType == typeof(double?):
                    return EntityProperty.GeneratePropertyForDouble((double?) property);

                case Type _ when targetPropertyType == typeof(Guid) || targetPropertyType == typeof(Guid?):
                    return EntityProperty.GeneratePropertyForGuid((Guid?) property);

                case Type _ when targetPropertyType == typeof(byte[]):
                    return EntityProperty.GeneratePropertyForByteArray((byte[]) property);

                default:
                    var @string = property != null
                        ? property.ToString()
                        : AzureTableStorageRepository.NullValue;
                    return EntityProperty.GeneratePropertyForString(@string);
            }
        }

        private static bool IsMinimumAllowableDate(this DateTime dateTime,
            AzureTableStorageRepository.TableStorageApiOptions options)
        {
            return dateTime == options.MinimumAllowableUtcDateTime;
        }

        public static IReadOnlyDictionary<string, object> FromTableEntity(this DynamicTableEntity tableEntity,
            RepositoryEntityMetadata metadata,
            AzureTableStorageRepository.TableStorageApiOptions options)

        {
            var containerEntityProperties = tableEntity.Properties
                .Where(pair =>
                    metadata.HasType(pair.Key) && pair.Value.PropertyAsObject != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromTableEntityProperty(metadata.GetPropertyType(pair.Key), options));

            var id = tableEntity.RowKey;
            containerEntityProperties[nameof(CommandEntity.Id)] = id;

            return containerEntityProperties;
        }

        private static object FromTableEntityProperty(this EntityProperty property, Type targetPropertyType,
            AzureTableStorageRepository.TableStorageApiOptions options)
        {
            var value = property.PropertyAsObject;
            switch (value)
            {
                case string text:
                    if (text.EqualsOrdinal(AzureTableStorageRepository.NullValue))
                    {
                        return null;
                    }

                    if (targetPropertyType.IsComplexStorageType())
                    {
                        return text.ComplexTypeFromContainerProperty(targetPropertyType);
                    }

                    return text;

                case DateTime dateTime:
                    if (targetPropertyType == typeof(DateTimeOffset) || targetPropertyType == typeof(DateTimeOffset?))
                    {
                        var dateTimeOffset = property.DateTimeOffsetValue.GetValueOrDefault(DateTimeOffset.MinValue);
                        return !dateTimeOffset.DateTime.HasValue() ||
                               dateTimeOffset.DateTime.IsMinimumAllowableDate(options)
                            ? DateTimeOffset.MinValue.UtcDateTime
                            : dateTimeOffset;
                    }
                    else
                    {
                        return !dateTime.HasValue() || dateTime.IsMinimumAllowableDate(options)
                            ? DateTime.MinValue
                            : dateTime;
                    }

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
    }

    // ReSharper disable once InconsistentNaming
    public static class AzureTableStorageWhereExtensions
    {
        public static string ToAzureTableStorageWhereClause(this IEnumerable<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToAzureTableStorageWhereClause());
            }

            return builder.ToString();
        }

        private static string ToAzureTableStorageWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToAzureTableStorageWhereClause()}{condition.ToAzureTableStorageWhereClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToAzureTableStorageWhereClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToAzureTableStorageWhereClause()}");
                }

                builder.Append(")");

                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToAzureTableStorageWhereClause(this LogicalOperator op)
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

        private static string ToAzureTableStorageWhereClause(this ConditionOperator op)
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

        private static string ToAzureTableStorageWhereClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName.EqualsOrdinal(nameof(QueryEntity.Id))
                ? TableConstants.RowKey
                : condition.FieldName;
            var conditionOperator = condition.Operator.ToAzureTableStorageWhereClause();

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
                        AzureTableStorageRepository.NullValue);

                default:
                    if (value is IPersistableValueObject valueObject)
                    {
                        return TableQuery.GenerateFilterCondition(fieldName, conditionOperator,
                            valueObject.Dehydrate());
                    }

                    return TableQuery.GenerateFilterCondition(fieldName, conditionOperator, value.ToJson());
            }
        }
    }

    // ReSharper disable once InconsistentNaming
    internal static class AzureTableStorageExceptionExtensions
    {
        public static bool IsTableNotFoundException(this StorageException exception)
        {
            var info = exception.RequestInformation;

            return info.HttpStatusCode == (int) HttpStatusCode.NotFound
                   && info.ExtendedErrorInformation.ErrorCode.EqualsOrdinal(TableErrorCodeStrings.TableNotFound);
        }

        public static bool IsEntityNotFoundException(this StorageException exception)
        {
            var info = exception.RequestInformation;

            return info.HttpStatusCode == (int) HttpStatusCode.NotFound
                   && info.ExtendedErrorInformation.ErrorCode.EqualsOrdinal(TableErrorCodeStrings.EntityNotFound);
        }

        public static Exception ToDetailedException(this StorageException storageException)
        {
            if (storageException.RequestInformation != null)
            {
                var errorMessage = storageException.RequestInformation.ExtendedErrorInformation?.ErrorMessage;
                var additionalDetails = storageException.RequestInformation.ExtendedErrorInformation?.AdditionalDetails
                    ?.ToJson();
                var statusCode = storageException.RequestInformation.HttpStatusCode;

                return ToDetailedException(storageException, errorMessage, additionalDetails, statusCode);
            }

            return storageException;
        }

        private static Exception ToDetailedException(Exception exception, string errorMessage, string additionalDetails,
            int statusCode)
        {
            var message = $"{exception.Message}: {errorMessage}. Details: {additionalDetails}";

            if (statusCode == (int) HttpStatusCode.NotFound)
            {
                return new ResourceNotFoundException(message, exception);
            }

            if (statusCode == (int) HttpStatusCode.Conflict)
            {
                return new ResourceConflictException(message, exception);
            }

            return new StorageException(message, exception);
        }
    }
}