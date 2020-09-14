using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Domain.Interfaces.Entities;
using Microsoft.Data.SqlClient;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack.Configuration;

namespace Storage.Sql
{
    public class SqlServerRepository : IRepository
    {
        internal const string PrimaryTableAlias = @"t";

        internal const string JoinedEntityFieldAliasPrefix = @"je_";

        public static readonly DateTime MinimumAllowableDate = SqlDateTime.MinValue.Value;

        private readonly string connectionString;

        public SqlServerRepository(string connectionString)
        {
            connectionString.GuardAgainstNullOrEmpty(nameof(connectionString));
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

        public static SqlServerRepository FromSettings(IAppSettings settings, string databaseName)
        {
            settings.GuardAgainstNull(nameof(settings));
            databaseName.GuardAgainstNull(nameof(databaseName));

            var serverName = settings.GetString("SqlServerDbServerName");
            var credentials = settings.GetString("SqlServerDbCredentials");
            return new SqlServerRepository(
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
                return Enumerable.Range(0, sqlDataReader.FieldCount)
                    .ToDictionary(sqlDataReader.GetName, sqlDataReader.GetValue);
            }

            using (var connection = new SqlConnection(this.connectionString))
            {
                var results = new List<Dictionary<string, object>>();
                try
                {
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
                            return results;
                        }
                    }
                }
                catch (SqlException)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    return null;
                }
            }
        }

        private Dictionary<string, object> ExecuteSingleSelect(string commandText, bool throwOnError = true)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }
                            return Enumerable.Range(0, reader.FieldCount)
                                .ToDictionary(x => reader.GetName(x), x => reader.GetValue(x));
                        }
                    }
                }
                catch (SqlException)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    return null;
                }
            }
        }

        private void ExecuteInsert(string tableName, Dictionary<string, object> properties, bool throwOnError = true)
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
                }
                catch (Exception)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
        }

        private void ExecuteUpdate(string tableName, Dictionary<string, object> properties, bool throwOnError = true)
        {
            var columnIndex = 1;
            var columnValueExpressions = string.Join(',', properties.Select(p => $"{p.Key}=@{columnIndex++}"));
            var commandText = $"UPDATE {tableName} SET {columnValueExpressions}";

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
                }
                catch (Exception)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
        }

        private object ExecuteScalarCommand(string commandText, bool throwOnError = true)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        return command.ExecuteScalar();
                    }
                }
                catch (SqlException)
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    return null;
                }
            }
        }

        private void ExecuteCommand(string commandText, bool throwOnError = true)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                ExecuteCommand(commandText, connection, throwOnError);
            }
        }

        private static void ExecuteCommand(string commandText, SqlConnection connection, bool throwOnError = true)
        {
            try
            {
                connection.Open();
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            catch (SqlException)
            {
                if (throwOnError)
                {
                    throw;
                }
            }
        }
    }

    public static class SqlServerExtensions
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

        private static object ToTableEntityProperty(object propertyValue, Type targetPropertyType)
        {
            var value = propertyValue;
            if (value != null)
            {
                if (value.GetType().IsComplexStorageType())
                {
                    value = value.ToString();
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

        public static bool IsDbNull(this object value)
        {
            return value == null
                   || value == DBNull.Value
                   || value is SqlBinary binary && binary.IsNull;
        }

        private static bool IsMinimumAllowableDate(this DateTime dateTime)
        {
            return dateTime == SqlServerRepository.MinimumAllowableDate;
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

    public static class SqlServerQueryExtensions
    {
        public static string ToSqlServerQueryClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query,
            string tableName,
            IRepository repository)
            where TQueryableEntity : IQueryableEntity
        {
            var builder = new StringBuilder();
            builder.Append($"SELECT {query.ToSqlServerSelectClause()}");
            builder.Append($" FROM {tableName} {SqlServerRepository.PrimaryTableAlias}");

            var joins = query.JoinedEntities.ToSqlServerJoinClause();
            if (joins.HasValue())
            {
                builder.Append($"{joins}");
            }

            var wheres = query.Wheres.ToSqlServerWhereClause();
            if (wheres.HasValue())
            {
                builder.Append($" WHERE {wheres}");
            }

            var orderBy = query.ToSqlServerOrderByClause();
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
        private static string ToSqlServerSelectClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
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

        private static string ToSqlServerOrderByClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity
        {
            var orderBy = query.ResultOptions.OrderBy;
            var direction = orderBy.Direction == OrderDirection.Ascending
                ? "ASC"
                : "DESC";
            var by = query.GetDefaultOrdering();

            return $"{SqlServerRepository.PrimaryTableAlias}.{by} {direction}";
        }

        private static string ToSqlServerWhereClause(this IReadOnlyList<WhereExpression> wheres)
        {
            if (!wheres.Any())
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToSqlServerWhereClause());
            }

            return builder.ToString();
        }

        private static string ToSqlServerJoinClause(this IReadOnlyList<QueriedEntity> joinedEntities)
        {
            if (!joinedEntities.Any())
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var entity in joinedEntities)
            {
                builder.Append(entity.Join.ToSqlServerJoinClause());
            }

            return builder.ToString();
        }

        private static string ToSqlServerJoinClause(this JoinDefinition join)
        {
            var joinType = join.Type.ToSqlServerJoinType();

            return
                $" {joinType} JOIN {join.Right.EntityName} ON {SqlServerRepository.PrimaryTableAlias}.{join.Left.JoinedFieldName}={join.Right.EntityName}.{join.Right.JoinedFieldName}";
        }

        private static string ToSqlServerJoinType(this JoinType type)
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

        private static string ToSqlServerWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToSqlServerOperatorClause()}{condition.ToSqlServerConditionClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToSqlServerOperatorClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToSqlServerWhereClause()}");
                }

                builder.Append(")");

                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToSqlServerOperatorClause(this LogicalOperator op)
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

        private static string ToSqlServerConditionClause(this ConditionOperator op)
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

        private static string ToSqlServerConditionClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName;
            var @operator = condition.Operator.ToSqlServerConditionClause();

            var value = condition.Value;
            switch (value)
            {
                case string text:
                    return $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{text}'";

                case DateTime dateTime:
                    return dateTime.HasValue()
                        ? $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{dateTime:yyyy-MM-dd HH:mm:ss.fff}'"
                        : $"({SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{SqlServerRepository.MinimumAllowableDate:yyyy-MM-dd HH:mm:ss.fff}' OR {SqlServerRepository.PrimaryTableAlias}.{fieldName} {(condition.Operator == ConditionOperator.EqualTo ? "IS" : @operator)} NULL)";

                case DateTimeOffset dateTimeOffset:
                    return
                        $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{dateTimeOffset:O}'";

                case bool boolean:
                    return
                        $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} {(boolean ? 1 : 0)}";

                case double _:
                case int _:
                case long _:
                    return $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} {value}";

                case byte[] bytes:
                    return
                        $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} {ToHexByteArray(bytes)}";

                case Guid guid:
                    return $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{guid:D}'";

                case null:
                    return condition.Operator == ConditionOperator.EqualTo
                        ? $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} IS NULL"
                        : $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} IS NOT NULL";

                default:
                    return value.ToOtherValueString(fieldName, @operator);
            }
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

        private static string ToOtherValueString(this object value, string fieldName, string @operator)
        {
            if (value == null)
            {
                return $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} NULL";
            }

            if (value is IPersistableValueObject valueObject)
            {
                return
                    $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{valueObject.Dehydrate()}'";
            }

            return $"{SqlServerRepository.PrimaryTableAlias}.{fieldName} {@operator} '{value}'";
        }
    }
}