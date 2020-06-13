using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public abstract class InMemStorage<TEntity> : IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        private readonly InMemEntityRepository store;

        protected InMemStorage(InMemEntityRepository store)
        {
            Guard.AgainstNull(() => store, store);

            this.store = store;
        }

        protected abstract string EntityName { get; }

        public string Add(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            return this.store.Add(EntityName, entity);
        }

        public void Delete(string id, bool ignoreConcurrency)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            this.store.Remove(EntityName, id);
        }

        public TEntity Get(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            return (TEntity) this.store.Get(EntityName, id);
        }

        public TEntity Update(TEntity entity, bool ignoreConcurrency)
        {
            Guard.AgainstNull(() => entity, entity);
            if (!entity.Id.HasValue())
            {
                throw new EntityNotIdentifiedException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                throw new EntityNotExistsException("Entity not found");
            }

            latest.PopulateWith(entity);

            this.store.Update(EntityName, entity.Id, entity);
            return entity;
        }

        public long Count()
        {
            return this.store.Count(EntityName);
        }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options)
        {
            Guard.AgainstNull(() => query, query);

            if (query == null || query.Options.IsEmpty)
            {
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var resultEntities = this.store.GetAll(EntityName)
                .Cast<TEntity>()
                .ToList();

            if (query.Wheres.Any())
            {
                var queryExpression = query.Wheres.ToDynamicLinq();
                resultEntities = resultEntities.AsQueryable()
                    .Where(queryExpression)
                    .ToList();
            }

            // TODO: Joins
            foreach (var queriedEntity in query.Entities.Where(e => e.Join != null))
            {
                foreach (var resultEntity in resultEntities)
                {
                    //TODO: Fetch the first entity that matches the join 
                }
            }

            //TODO: selects, resolve any joins, select only selected fields

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }

    public static class WhereExpressionExtensions
    {
        public static string ToDynamicLinq(this IReadOnlyList<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToDynamicLinq());
            }

            return builder.ToString();
        }

        private static string ToDynamicLinq(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;
                return
                    $"{where.Operator.ToDynamicLinq()}{condition.FieldName} {condition.Operator.ToDynamicLinq()} {condition.Value.ToDynamicLinq()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToDynamicLinq()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToDynamicLinq()}");
                }

                builder.Append(")");
                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToDynamicLinq(this ConditionOperator op)
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

        private static string ToDynamicLinq(this LogicalOperator op)
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

        private static string ToDynamicLinq(this object value)
        {
            if (value is DateTime dateTime)
            {
                return $"\"{dateTime:O}\"";
            }

            return $"\"{value}\"";
        }
    }
}