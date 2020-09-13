using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Text;
using QueryAny;
using QueryAny.Primitives;

namespace Storage
{
    /// <summary>
    ///     See reference: https://dynamic-linq.net/expression-language
    /// </summary>
    public static class DynamicLinqWhereExtensions
    {
        public static string ToDynamicLinqOrderByClause<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity

        {
            var orderBy = query.GetDefaultOrdering().ToDynamicLinqFieldName();
            orderBy = $"{orderBy}{(query.ResultOptions.OrderBy.Direction == OrderDirection.Descending ? " DESC" : "")}";

            return orderBy;
        }

        public static string ToDynamicLinqWhereClause(this IEnumerable<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToDynamicLinqWhereClause());
            }

            return builder.ToString();
        }

        private static string ToDynamicLinqWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToDynamicLinqWhereClause()}{condition.ToDynamicLinqWhereClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToDynamicLinqWhereClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToDynamicLinqWhereClause()}");
                }

                builder.Append(")");

                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToDynamicLinqWhereClause(this LogicalOperator op)
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

        private static string ToDynamicLinqWhereClause(this ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.EqualTo:
                    return "==";
                case ConditionOperator.GreaterThan:
                    return ">";
                case ConditionOperator.GreaterThanEqualTo:
                    return ">=";
                case ConditionOperator.LessThan:
                    return "<";
                case ConditionOperator.LessThanEqualTo:
                    return "<=";
                case ConditionOperator.NotEqualTo:
                    return "!=";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToDynamicLinqFieldName(this string fieldName)
        {
            return $"Value[\"{fieldName}\"]";
        }

        private static string ToDynamicLinqWhereClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName.ToDynamicLinqFieldName();
            var @operator = condition.Operator.ToDynamicLinqWhereClause();
            var value = condition.Value;

            if (value is string)
            {
                return $"String({fieldName}) {@operator} \"{value}\"";
            }

            if (value is bool boolean)
            {
                return $"Boolean({fieldName}) {@operator} {boolean.ToLower()}";
            }

            if (value is DateTime dateTime)
            {
                return
                    $"DateTime({fieldName}) {@operator} DateTime({dateTime.Ticks}, DateTimeKind.Utc)";
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return
                    $"DateTimeOffset({fieldName}) {@operator} DateTimeOffset({dateTimeOffset.Ticks}, TimeSpan.Parse(\"{dateTimeOffset.Offset.ToString()}\"))";
            }

            if (value is byte[] bytes)
            {
                return
                    $"Convert.ToBase64String(DynamicLinqUtils.ToByteArray({fieldName})) {@operator} \"{Convert.ToBase64String(bytes)}\"";
            }

            if (value is Guid)
            {
                return $"Guid({fieldName}) {@operator} \"{value}\"";
            }

            if (value is int)
            {
                return $"Int32({fieldName}) {@operator} {value}";
            }

            if (value is long)
            {
                return $"Int64({fieldName}) {@operator} {value}";
            }

            if (value is double)
            {
                return $"Double({fieldName}) {@operator} {value}";
            }

            return value.OtherTypeToString(fieldName, @operator);
        }

        private static string OtherTypeToString(this object value, string fieldName, string @operator)
        {
            if (value == null)
            {
                return $"iif ({fieldName} != null, {fieldName}.ToString(), null) {@operator} null";
            }

            var escapedValue = value
                .ToString()
                .Replace("\"", "\\\"");

            return $"iif ({fieldName} != null, {fieldName}.ToString(), null) {@operator} \"{escapedValue}\"";
        }
    }

    /// <summary>
    ///     Extension used in <see cref="DynamicLinqWhereExtensions.ToDynamicLinqWhereClause" /> to do conversion.
    ///     See: https://dynamic-linq.net/advanced-extending
    /// </summary>
    [DynamicLinqType]

    // ReSharper disable once UnusedType.Global
    public static class DynamicLinqUtils
    {
        // ReSharper disable once UnusedMember.Global
        public static byte[] ToByteArray(object values)
        {
            return (byte[]) values;
        }
    }
}