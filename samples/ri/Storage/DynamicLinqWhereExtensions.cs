using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Text;
using Domain.Interfaces;
using QueryAny;

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
            var orderBy = query.GetDefaultOrdering().ToFieldName();
            orderBy = $"{orderBy}{(query.ResultOptions.OrderBy.Direction == OrderDirection.Descending ? " DESC" : "")}";

            return orderBy;
        }

        public static string ToDynamicLinqWhereClause(this IEnumerable<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToWhereClause());
            }

            return builder.ToString();
        }

        private static string ToWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;

                return
                    $"{where.Operator.ToOperatorClause()}{condition.ToConditionClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToOperatorClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToWhereClause()}");
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
                    return " and ";
                case LogicalOperator.Or:
                    return " or ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToConditionClause(this ConditionOperator op)
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

        private static string ToFieldName(this string fieldName)
        {
            return $"Value[\"{fieldName}\"]";
        }

        private static string ToConditionClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName.ToFieldName();
            var @operator = condition.Operator.ToConditionClause();
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

            return value.ToWhereConditionOtherValueString(fieldName, @operator);
        }

        private static string ToWhereConditionOtherValueString(this object value, string fieldName, string @operator)
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