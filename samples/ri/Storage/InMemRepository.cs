using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class InMemRepository
    {
        private readonly Dictionary<string, Dictionary<string, IKeyedEntity>> containers =
            new Dictionary<string, Dictionary<string, IKeyedEntity>>();

        private readonly IIdentifierFactory idFactory;

        public InMemRepository(IIdentifierFactory idFactory)
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
            this.containers[containerName].Add(entity.Id, entity);
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
                    this.containers[containerName][id] = entity;
                }
            }
        }

        public IKeyedEntity Get(string containerName, string id)
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    return this.containers[containerName][id];
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

        public IEnumerable<IKeyedEntity> GetAll(string containerName)
        {
            if (this.containers.ContainsKey(containerName))
            {
                return this.containers[containerName].Values;
            }

            return Enumerable.Empty<IKeyedEntity>();
        }

        public void DestroyAll(string containerName)
        {
            if (this.containers.ContainsKey(containerName))
            {
                this.containers.Remove(containerName);
            }
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
                    $"{where.Operator.ToDynamicLinqWhereClause()}{condition.FieldName} {condition.Operator.ToDynamicLinqWhereClause()} {condition.Value.ToDynamicLinqWhereClause()}";
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

        private static string ToDynamicLinqWhereClause(this object value)
        {
            if (value is DateTime dateTime)
            {
                return $"\"{dateTime:O}\"";
            }

            return $"\"{value}\"";
        }
    }
}