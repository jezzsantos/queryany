using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;
using StringExtensions = ServiceStack.StringExtensions;

namespace Storage
{
    public class InProcessInMemRepository
    {
        private readonly Dictionary<string, Dictionary<string, IKeyedEntity>> containers =
            new Dictionary<string, Dictionary<string, IKeyedEntity>>();

        private readonly IIdentifierFactory idFactory;

        public InProcessInMemRepository(IIdentifierFactory idFactory)
        {
            Guard.AgainstNull(() => idFactory, idFactory);
            this.idFactory = idFactory;
        }

        public string Add(string containerName, IKeyedEntity entity)
        {
            if (!this.containers.ContainsKey(containerName))
            {
                this.containers.Add(containerName, new Dictionary<string, IKeyedEntity>());
            }

            var id = this.idFactory.Create(entity);
            entity.Id = id;
            this.containers[containerName].Add(entity.Id, entity.ToRepositoryType());
            return id;
        }

        public void Remove(string containerName, string id)
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    this.containers[containerName].Remove(id);
                }
            }
        }

        public void Update(string containerName, string id, IKeyedEntity entity)
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    this.containers[containerName][id] = entity.ToRepositoryType();
                }
            }
        }

        public IKeyedEntity Get(string containerName, string id)
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    return this.containers[containerName][id].FromRepositoryType();
                }
            }

            return null;
        }

        public long Count(string containerName)
        {
            if (this.containers.ContainsKey(containerName))
            {
                return this.containers[containerName].Count;
            }

            return 0;
        }

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IKeyedEntity, new()
        {
            if (!this.containers.ContainsKey(containerName))
            {
                return new List<TEntity>();
            }

            var resultEntities = this.containers[containerName].Values
                .ToList()
                .ConvertAll(e => (TEntity) e.FromRepositoryType());

            if (query.Wheres.Any())
            {
                var queryExpression = query.Wheres.ToDynamicLinqWhereClause();
                resultEntities = resultEntities.AsQueryable()
                    .Where(queryExpression)
                    .ToList();
            }

            // TODO: Join + SelectFromJoin

            if (query.PrimaryEntity.Selects.Any())
            {
                return PruneSelectedProperties(query.PrimaryEntity.Selects, resultEntities);
            }

            return resultEntities;
        }

        public void DestroyAll(string containerName)
        {
            if (this.containers.ContainsKey(containerName))
            {
                this.containers.Remove(containerName);
            }
        }

        private static List<TEntity> PruneSelectedProperties<TEntity>(IReadOnlyList<SelectDefinition> selects,
            IEnumerable<TEntity> resultEntities) where TEntity : IKeyedEntity, new()
        {
            var selectedResultEntities = new List<TEntity>();
            var selectedPropNames = selects.Select(select => select.FieldName);
            foreach (var resultEntity in resultEntities)
            {
                var properties = resultEntity.ToObjectDictionary()
                    .Where(prop =>
                        selectedPropNames.Contains(prop.Key) ||
                        StringExtensions.EqualsIgnoreCase(prop.Key, nameof(IKeyedEntity.Id)));
                selectedResultEntities.Add(properties.FromObjectDictionary<TEntity>());
            }

            return selectedResultEntities;
        }
    }

    public static class InMemEntityExtensions
    {
        public static IKeyedEntity ToRepositoryType(this IKeyedEntity entity)
        {
            return entity;
        }

        public static IKeyedEntity FromRepositoryType(this IKeyedEntity entity)
        {
            return entity;
        }
    }

    public static class DynamicLinqWhereExtensions
    {
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

        private static string ToDynamicLinqWhereClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName;
            var @operator = condition.Operator.ToDynamicLinqWhereClause();
            var value = condition.Value;

            if (value is DateTime dateTime)
            {
                return
                    $"{fieldName} {@operator} DateTime({dateTime.Ticks}, DateTimeKind.Utc)";
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return
                    $"{fieldName} {@operator} DateTimeOffset({dateTimeOffset.Ticks}, TimeSpan.Zero)";
            }

            if (value is byte[] bytes)
            {
                return $"Convert.ToBase64String({fieldName}) {@operator} \"{Convert.ToBase64String(bytes)}\"";
            }

            if (value is string || value is Guid)
            {
                return $"{fieldName} {@operator} \"{value}\"";
            }

            if (value is int || value is long || value is double)
            {
                return $"{fieldName} {@operator} {value}";
            }

            // Other Types (ComplexTypes must be equatable to their ToString() forms)
            return $"iif ({fieldName} != null, {fieldName}.ToString(), null) {@operator} {value.ToEscapedString()}";
        }

        private static string ToEscapedString(this object value)
        {
            if (value == null)
            {
                return "null";
            }

            var escapedJson = value
                .ToString()
                .Replace("\"", "\\\"");

            return $"\"{escapedJson}\"";
        }
    }
}