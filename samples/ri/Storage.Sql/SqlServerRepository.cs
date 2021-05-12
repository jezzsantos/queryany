using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Data.SqlClient;
using QueryAny;
using ServiceStack.Configuration;

namespace Storage.Sql
{
    public class SqlServerRepository : IRepository
    {
        internal const string PrimaryTableAlias = @"t";
        internal const string JoinedEntityFieldAliasPrefix = @"je_";
        public static readonly DateTime MinimumAllowableDate = SqlDateTime.MinValue.Value;
        public static readonly DateTime MaximumAllowableDate = SqlDateTime.MaxValue.Value;
        private readonly string connectionString;
        private readonly IRecorder recorder;

        private SqlServerRepository(IRecorder recorder, string connectionString)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
            this.recorder = recorder;
            this.connectionString = connectionString;
        }

        public int MaxQueryResults => 1000;

        public CommandEntity Add(string tableName, CommandEntity entity)
        {
            tableName.GuardAgainstNullOrEmpty(nameof(tableName));
            entity.GuardAgainstNull(nameof(entity));

            ExecuteInsert(tableName, entity.ToTableEntity());

            return Retrieve(tableName, entity.Id, entity.Metadata);
        }

        public void Remove(string tableName, string id)
        {
            tableName.GuardAgainstNullOrEmpty(nameof(tableName));
            id.GuardAgainstNull(nameof(id));

            ExecuteCommand($"DELETE FROM {tableName} WHERE {nameof(CommandEntity.Id)}='{id}'");
        }

        public CommandEntity Retrieve(string tableName, string id, RepositoryEntityMetadata metadata)
        {
            tableName.GuardAgainstNullOrEmpty(nameof(tableName));
            id.GuardAgainstNull(nameof(id));
            metadata.GuardAgainstNull(nameof(metadata));

            var tableEntity =
                ExecuteSingleSelect($"SELECT * FROM {tableName} WHERE {nameof(CommandEntity.Id)}='{id}'");
            if (tableEntity != null)
            {
                return CommandEntity.FromCommandEntity(tableEntity.FromTableEntity(metadata), metadata);
            }

            return default;
        }

        public CommandEntity Replace(string tableName, string id, CommandEntity entity)
        {
            tableName.GuardAgainstNullOrEmpty(nameof(tableName));
            id.GuardAgainstNull(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var updatedEntity = entity.ToTableEntity();
            ExecuteUpdate(tableName, updatedEntity);

            return CommandEntity.FromCommandEntity(updatedEntity.FromTableEntity(entity.Metadata), entity);
        }

        public long Count(string tableName)
        {
            tableName.GuardAgainstNullOrEmpty(nameof(tableName));

            return (int) ExecuteScalarCommand($"SELECT COUNT({nameof(CommandEntity.Id)}) FROM {tableName}");
        }

        public List<QueryEntity> Query<TQueryableEntity>(string tableName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            if (query == null || query.Options.IsEmpty)
            {
                return new List<QueryEntity>();
            }

            var take = query.GetDefaultTake(this);
            if (take == 0)
            {
                return new List<QueryEntity>();
            }

            var select = query.ToSqlServerQueryClause(tableName, this);
            var results = ExecuteMultiSelect(select);

            return results
                .Select(properties => QueryEntity.FromProperties(properties.FromTableEntity(metadata), metadata))
                .ToList();
        }

        public void DestroyAll(string tableName)
        {
            tableName.GuardAgainstNullOrEmpty(nameof(tableName));

            ExecuteCommand($"DELETE FROM {tableName}");
        }

        public static SqlServerRepository FromSettings(IRecorder recorder, IAppSettings settings)
        {
            settings.GuardAgainstNull(nameof(settings));

            var serverName = settings.GetString("SqlServerDbServerName");
            var credentials = settings.GetString("SqlServerDbCredentials");
            var databaseName = settings.GetString("SqlServerDbName");
            return new SqlServerRepository(recorder,
                $"Persist Security Info=False;Integrated Security=true;Initial Catalog={databaseName};Server={serverName}{(credentials.HasValue() ? ";" + credentials : "")}");
        }

        private List<Dictionary<string, object>> ExecuteMultiSelect(string commandText, bool throwOnError = true)
        {
            static void OverwriteJoinedSelects(IDictionary<string, object> result)
            {
                var overwrites = new Dictionary<string, object>();
                foreach (var (key, value) in result)
                {
                    if (key.StartsWith(JoinedEntityFieldAliasPrefix))
                    {
                        var primaryFieldName = key.Remove(0, JoinedEntityFieldAliasPrefix.Length);
                        if (!value.IsDbNull())
                        {
                            overwrites.Add(primaryFieldName, value);
                        }
                    }
                }
                if (overwrites.Any())
                {
                    foreach (var (key, value) in overwrites)
                    {
                        result[key] = value;
                    }
                }
            }

            static Dictionary<string, object> ReaderToObjectDictionary(SqlDataReader sqlDataReader)
            {
                var result = new Dictionary<string, object>();
                Enumerable.Range(0, sqlDataReader.FieldCount)
                    .ToList()
                    .ForEach(column =>
                    {
                        var name = sqlDataReader.GetName(column);
                        if (!result.ContainsKey(name))
                        {
                            result.Add(name, sqlDataReader.GetValue(column));
                        }
                    });

                return result;
            }

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    var results = new List<Dictionary<string, object>>();
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var result = ReaderToObjectDictionary(reader);
                                OverwriteJoinedSelects(result);
                                results.Add(result);
                            }
                        }
                    }
                    this.recorder.TraceInformation($"Executed SQL statement: {commandText}");
                    return results;
                }
                catch (Exception ex)
                {
                    this.recorder.Crash(CrashLevel.NonCritical, ex, $"Failed executing SQL statement: {commandText}");
                    if (throwOnError)
                    {
                        throw;
                    }
                    return null;
                }
            }
        }

        private Dictionary<string, object> ExecuteSingleSelect(string commandText)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    Dictionary<string, object> result;
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }
                            result = Enumerable.Range(0, reader.FieldCount)
                                .ToDictionary(x => reader.GetName(x), x => reader.GetValue(x));
                        }
                    }
                    connection.Close();
                    this.recorder.TraceInformation($"Executed SQL statement: {commandText}");
                    return result;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed executing SQL statement: {commandText}", ex);
                }
            }
        }

        private void ExecuteInsert(string tableName, Dictionary<string, object> properties)
        {
            var columnNames = string.Join(',', properties.Select(p => p.Key));
            var columnIndex = 1;
            var columnValuePlaceholders = string.Join(',', properties.Select(p => $"@{columnIndex++}"));
            var commandText = $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValuePlaceholders})";

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        var index = 1;
                        foreach (var property in properties)
                        {
                            command.Parameters.AddWithValue($"@{index++}", property.Value);
                        }

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    this.recorder.TraceInformation($"Executed SQL statement: {commandText}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed executing SQL statement: {commandText}", ex);
                }
            }
        }

        private void ExecuteUpdate(string tableName, Dictionary<string, object> properties)
        {
            var columnIndex = 1;
            var columnValueExpressions = string.Join(',', properties.Select(p => $"{p.Key}=@{columnIndex++}"));
            var id = properties[nameof(CommandEntity.Id)];
            var commandText =
                $"UPDATE {tableName} SET {columnValueExpressions} WHERE {nameof(CommandEntity.Id)}='{id}'";

            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        var index = 1;
                        foreach (var property in properties)
                        {
                            command.Parameters.AddWithValue($"@{index++}", property.Value);
                        }

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    this.recorder.TraceInformation($"Executed SQL statement: {commandText}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed executing SQL statement: {commandText}", ex);
                }
            }
        }

        private object ExecuteScalarCommand(string commandText)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    object result;
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        result = command.ExecuteScalar();
                    }
                    connection.Close();
                    this.recorder.TraceInformation($"Executed SQL statement: {commandText}");

                    return result;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed executing SQL statement: {commandText}", ex);
                }
            }
        }

        private void ExecuteCommand(string commandText)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                ExecuteCommand(commandText, connection);
            }
        }

        private void ExecuteCommand(string commandText, SqlConnection connection)
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
                this.recorder.TraceInformation($"Executed SQL statement: {commandText}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed executing SQL statement: {commandText}", ex);
            }
        }
    }

    internal static class SqlServerRepositoryExtensions
    {
        public static Dictionary<string, object> ToTableEntity(this CommandEntity entity)
        {
            var tableEntityProperties = new Dictionary<string, object>();
            foreach (var (key, value) in entity.Properties)
            {
                var targetPropertyType = entity.GetPropertyType(key);
                tableEntityProperties.Add(key, ToTableEntityProperty(value, targetPropertyType));
            }

            tableEntityProperties[nameof(CommandEntity.LastPersistedAtUtc)] = DateTime.UtcNow;

            return tableEntityProperties;
        }

        public static Dictionary<string, object> FromTableEntity(this Dictionary<string, object> tableProperties,
            RepositoryEntityMetadata metadata)

        {
            var propertyValues = tableProperties
                .Where(pair =>
                    metadata.HasType(pair.Key) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.FromTableEntityProperty(metadata.GetPropertyType(pair.Key)));

            var id = tableProperties[nameof(CommandEntity.Id)].ToString();
            propertyValues[nameof(CommandEntity.Id)] = id;

            return propertyValues;
        }

        public static bool IsDbNull(this object value)
        {
            var isNullBinary = value is SqlBinary binary
                ? binary.IsNull
                : false;

            return value == null
                   || value == DBNull.Value
                   || isNullBinary;
        }

        public static bool IsMaximumAllowableDate(this DateTime dateTime)
        {
            return dateTime >= SqlServerRepository.MaximumAllowableDate;
        }

        private static object ToTableEntityProperty(object propertyValue, Type targetPropertyType)
        {
            var value = propertyValue;
            if (value != null)
            {
                if (targetPropertyType.IsEnum || targetPropertyType.IsNullableEnum())
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
                if (dateTime.HasValue())
                {
                    value = dateTime.IsNotAllowableDate()
                        ? SqlServerRepository.MinimumAllowableDate
                        : dateTime.ToUniversalTime();
                }
                else
                {
                    value = DBNull.Value;
                }
            }

            if (value is Guid guid)
            {
                value = guid.ToString("D");
            }

            if (value == null)
            {
                if (targetPropertyType == typeof(byte[]))
                {
                    value = SqlBinary.Null;
                }
                else
                {
                    value = DBNull.Value;
                }
            }

            return value;
        }

        private static object FromTableEntityProperty(this KeyValuePair<string, object> property,
            Type targetPropertyType)
        {
            var value = property.Value;
            switch (value)
            {
                case string text:

                    if (targetPropertyType == typeof(Guid) || targetPropertyType == typeof(Guid?))
                    {
                        return Guid.Parse(text);
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

                case DateTime dateTime:
                    return dateTime.IsMinimumAllowableDate()
                        ? DateTime.MinValue
                        : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                case DateTimeOffset _:
                case bool _:
                case int _:
                case long _:
                case double _:
                case byte[] _:
                    return value;

                case SqlBinary binary:
                    return binary.IsNull
                        ? null
                        : binary.Value;

                default:
                    if (value is DBNull)
                    {
                        if (targetPropertyType == typeof(DateTime))
                        {
                            return DateTime.MinValue;
                        }
                        return null;
                    }
                    throw new ArgumentOutOfRangeException(nameof(property));
            }
        }

        private static bool IsMinimumAllowableDate(this DateTime dateTime)
        {
            return dateTime <= SqlServerRepository.MinimumAllowableDate;
        }

        private static bool IsNotAllowableDate(this DateTime dateTime)
        {
            if (dateTime.HasValue())
            {
                return dateTime.ToUniversalTime() <= SqlServerRepository.MinimumAllowableDate;
            }

            return true;
        }
    }

    internal static class SqlServerQueryExtensions
    {
        public static string ToSqlServerQueryClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query,
            string tableName, IRepository repository)
            where TQueryableEntity : IQueryableEntity
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT {query.ToSelectClause()}");
            builder.Append($" FROM {tableName} {SqlServerRepository.PrimaryTableAlias}");

            var joins = query.JoinedEntities.ToJoinClause();
            if (joins.HasValue())
            {
                builder.Append($"{joins}");
            }

            var wheres = query.Wheres.ToWhereClause(query.JoinedEntities);
            if (wheres.HasValue())
            {
                builder.Append($" WHERE {wheres}");
            }

            var orderBy = query.ToOrderByClause(query.JoinedEntities);
            if (orderBy.HasValue())
            {
                builder.Append($" ORDER BY {orderBy}");
            }

            var skip = query.GetDefaultSkip();
            var take = query.GetDefaultTake(repository);
            builder.Append(take > 0
                ? $" OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY"
                : $" OFFSET {skip} ROWS");

            return builder.ToString();
        }

        /// <summary>
        ///     PrimarySelects?  SelectFromJoin?   Joins?      Result
        ///     No               No                No          *
        ///     No               Yes               Yes         GetJoinedSelectedAliasedProperties
        ///     Yes              No                No          GetPrimarySelectedProperties
        ///     Yes              Yes               Yes         GetPrimarySelectedProperties + GetJoinedSelectedAliasedProperties
        ///     Note: Cannot have SelectFromJoin without a corresponding Join defined.
        ///     No               No                Yes         GetAllPrimaryEntityProperties
        ///     Yes              No                Yes         GetPrimarySelectedProperties
        /// </summary>
        private static string ToSelectClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity
        {
            static string GetAllPrimaryEntityProperties()
            {
                return $"{SqlServerRepository.PrimaryTableAlias}.*";
            }

            string GetPrimarySelectedProperties()
            {
                var builder = new StringBuilder();
                builder.Append(
                    $"{SqlServerRepository.PrimaryTableAlias}.{nameof(QueryEntity.Id)}");
                foreach (var select in query.PrimaryEntity.Selects)
                {
                    if (select.FieldName.NotEqualsOrdinal(nameof(QueryEntity.Id)))
                    {
                        builder.Append($", {SqlServerRepository.PrimaryTableAlias}.{select.FieldName}");
                    }
                }

                return builder.ToString();
            }

            string GetJoinedSelectedAliasedProperties(bool includePrimary = false)
            {
                var builder = new StringBuilder();

                builder.Append(
                    $"{SqlServerRepository.PrimaryTableAlias}.{nameof(QueryEntity.Id)}");
                foreach (var join in query.JoinedEntities.SelectMany(je => je.Selects))
                {
                    if (includePrimary)
                    {
                        builder.Append(
                            $", {SqlServerRepository.PrimaryTableAlias}.{join.FieldName}");
                    }
                    builder.Append(
                        $", {join.EntityName}.{join.FieldName} AS {SqlServerRepository.JoinedEntityFieldAliasPrefix}{join.JoinedFieldName}");
                }

                return builder.ToString();
            }

            string GetPrimarySelectedPlusJoinedSelectedAliasedProperties()
            {
                var builder = new StringBuilder();

                builder.Append(GetPrimarySelectedProperties());
                builder.Append(", ");
                builder.Append(GetJoinedSelectedAliasedProperties());

                return builder.ToString();
            }

            if (query.PrimaryEntity.Selects.Any())
            {
                if (query.JoinedEntities.Any())
                {
                    if (query.JoinedEntities.SelectMany(je => je.Selects).Any())
                    {
                        return GetPrimarySelectedPlusJoinedSelectedAliasedProperties();
                    }
                    return GetPrimarySelectedProperties();
                }
                return GetPrimarySelectedProperties();
            }

            if (query.JoinedEntities.Any())
            {
                if (query.JoinedEntities.SelectMany(je => je.Selects).Any())
                {
                    return GetJoinedSelectedAliasedProperties(true);
                }
                return GetAllPrimaryEntityProperties();
            }

            return @"*";
        }

        private static string ToOrderByClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query,
            IReadOnlyList<QueriedEntity> joinedEntities)
            where TQueryableEntity : IQueryableEntity
        {
            var orderBy = query.ResultOptions.OrderBy;
            var direction = orderBy.Direction == OrderDirection.Ascending
                ? "ASC"
                : "DESC";
            var by = query.GetDefaultOrdering();

            var fieldName = ToOrderByFieldName(by, joinedEntities);
            return $"{fieldName} {direction}";
        }

        private static string ToOrderByFieldName(string by, IReadOnlyList<QueriedEntity> joinedEntities)
        {
            if (joinedEntities.Any())
            {
                var joinedField = joinedEntities
                    .SelectMany(je => je.Selects)
                    .FirstOrDefault(sel => sel.JoinedFieldName.EqualsIgnoreCase(by));
                if (joinedField.Exists())
                {
                    var joinLeftFieldName =
                        $"{SqlServerRepository.JoinedEntityFieldAliasPrefix}{joinedField?.JoinedFieldName}";
                    return joinLeftFieldName;
                }
            }

            return $"{SqlServerRepository.PrimaryTableAlias}.{by}";
        }

        private static string ToWhereClause(this IReadOnlyList<WhereExpression> wheres,
            IReadOnlyList<QueriedEntity> joinedEntities)
        {
            if (!wheres.Any())
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToWhereClause(joinedEntities));
            }

            return builder.ToString();
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
                $" {joinType} JOIN {join.Right.EntityName} ON {SqlServerRepository.PrimaryTableAlias}.{join.Left.JoinedFieldName}={join.Right.EntityName}.{join.Right.JoinedFieldName}";
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

        private static string ToWhereClause(this WhereExpression where,
            IReadOnlyList<QueriedEntity> joinedEntities)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToOperatorClause()}{condition.ToConditionClause(joinedEntities)}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToOperatorClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToWhereClause(joinedEntities)}");
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

        private static string ToConditionClause(this WhereCondition condition,
            IReadOnlyList<QueriedEntity> joinedEntities)
        {
            var fieldName = ToFieldName(condition.FieldName, joinedEntities);
            var @operator = condition.Operator.ToConditionClause();

            var value = condition.Value;
            switch (value)
            {
                case string text:
                    return $"{fieldName} {@operator} '{text}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? dateTime.IsMaximumAllowableDate()
                            ? $"{fieldName} {@operator} '{SqlServerRepository.MaximumAllowableDate:yyyy-MM-dd HH:mm:ss.fff}'"
                            : $"{fieldName} {@operator} '{dateTime:yyyy-MM-dd HH:mm:ss.fff}'"
                        : $"({fieldName} {@operator} '{SqlServerRepository.MinimumAllowableDate:yyyy-MM-dd HH:mm:ss.fff}' OR {fieldName} {(condition.Operator == ConditionOperator.EqualTo ? "IS" : @operator)} NULL)";

                case DateTimeOffset dateTimeOffset:
                    return
                        $"{fieldName} {@operator} '{dateTimeOffset:O}'";

                case bool boolean:
                    return
                        $"{fieldName} {@operator} {(boolean ? 1 : 0)}";

                case double _:
                case int _:
                case long _:
                    return $"{fieldName} {@operator} {value}";

                case byte[] bytes:
                    return
                        $"{fieldName} {@operator} {ToHexByteArray(bytes)}";

                case Guid guid:
                    return $"{fieldName} {@operator} '{guid:D}'";

                case null:
                    return condition.Operator == ConditionOperator.EqualTo
                        ? $"{fieldName} IS NULL"
                        : $"{fieldName} IS NOT NULL";

                default:
                    return value.ToWhereConditionOtherValueString(fieldName, @operator);
            }
        }

        private static string ToConditionClause(this ConditionOperator op)
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

        private static string ToFieldName(string fieldName, IReadOnlyList<QueriedEntity> joinedEntities)
        {
            if (joinedEntities.Any())
            {
                var joinedField = joinedEntities
                    .SelectMany(je => je.Selects)
                    .FirstOrDefault(sel => sel.JoinedFieldName.EqualsIgnoreCase(fieldName));
                if (joinedField.Exists())
                {
                    var joinLeftFieldName = $"{joinedField?.EntityName}.{joinedField?.FieldName}";
                    return joinLeftFieldName;
                }
            }

            return $"{SqlServerRepository.PrimaryTableAlias}.{fieldName}";
        }

        private static string ToHexByteArray(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return "NULL";
            }

            var sequence = BitConverter.ToString(bytes).Replace("-", "");
            return $"0x{sequence}";
        }

        private static string ToWhereConditionOtherValueString(this object value, string fieldName, string @operator)
        {
            if (value == null)
            {
                return $"{fieldName} {@operator} NULL";
            }

            if (value is IPersistableValueObject valueObject)
            {
                return
                    $"{fieldName} {@operator} '{valueObject.Dehydrate()}'";
            }

            return $"{fieldName} {@operator} '{value}'";
        }
    }
}