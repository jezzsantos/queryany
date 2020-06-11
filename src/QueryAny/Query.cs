using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;

namespace QueryAny
{
    public static class Query
    {
        public static FromClause<TEntity> From<TEntity>() where TEntity : INamedEntity, new()
        {
            var entity = new TEntity();
            return new FromClause<TEntity>(entity);
        }

        public static WhereClause<TEntity> Empty<TEntity>() where TEntity : INamedEntity, new()
        {
            var entity = new TEntity();
            return new WhereClause<TEntity>(new Collection<TEntity>(entity));
        }
    }

    public class FromClause<TEntity> where TEntity : INamedEntity, new()
    {
        public FromClause(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            Collection = new Collection<TEntity>(entity);
        }

        public Collection<TEntity> Collection { get; }

        public WhereClause<TEntity> Where<TValue>(Expression<Func<TEntity, TValue>> propertyName, Condition condition,
            TValue value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            Collection.AddExpression(Combine.None, columnName, condition, value);
            return new WhereClause<TEntity>(Collection);
        }
    }

    public class WhereClause<TEntity> where TEntity : INamedEntity, new()
    {
        private readonly Collection<TEntity> collection;

        public WhereClause(Collection<TEntity> collection)
        {
            Guard.AgainstNull(() => collection, collection);
            this.collection = collection;
            Collections = new List<Collection<TEntity>> {this.collection}.AsReadOnly();
        }

        public IReadOnlyList<Collection<TEntity>> Collections { get; }

        public WhereClause<TEntity> AndWhere<TValue>(Expression<Func<TEntity, TValue>> propertyName,
            Condition condition,
            TValue value)
        {
            if (!this.collection.Expressions.Any())
            {
                throw new InvalidOperationException("Must have at least one expression to AND with");
            }

            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            this.collection.AddExpression(Combine.And, columnName, condition, value);
            return new WhereClause<TEntity>(this.collection);
        }

        public WhereClause<TEntity> OrWhere<TValue>(Expression<Func<TEntity, TValue>> propertyName, Condition condition,
            TValue value)
        {
            if (!this.collection.Expressions.Any())
            {
                throw new InvalidOperationException("Must have at least one expression to OR with");
            }

            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            this.collection.AddExpression(Combine.Or, columnName, condition, value);
            return new WhereClause<TEntity>(this.collection);
        }

        public WhereClause<TEntity> AndWhere(Func<FromClause<TEntity>, WhereClause<TEntity>> subWhereClause)
        {
            if (!this.collection.Expressions.Any())
            {
                throw new InvalidOperationException("Must have at least one expression to AND with");
            }

            var fromClause = new FromClause<TEntity>(this.collection.UnderlyingEntity);
            subWhereClause(fromClause);

            this.collection.AddExpression(Combine.And, fromClause.Collection.Expressions.ToList());
            return new WhereClause<TEntity>(this.collection);
        }
    }

    public class Collection<TEntity> where TEntity : INamedEntity, new()
    {
        private const string EntityTypeNameConventionSuffix = @"Entity";
        private readonly TEntity entity;
        private readonly List<QueryExpression> expressions;

        public Collection(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            this.entity = entity;
            this.expressions = new List<QueryExpression>();
        }

        public string Name => GetEntityName();

        internal TEntity UnderlyingEntity => this.entity;

        public IReadOnlyList<QueryExpression> Expressions => this.expressions.AsReadOnly();

        private string GetEntityName()
        {
            if (this.entity.EntityName.HasValue())
            {
                return this.entity.EntityName;
            }

            var name = this.entity.GetType().Name;
            return name.EndsWith(EntityTypeNameConventionSuffix)
                ? name.Substring(0, name.LastIndexOf(EntityTypeNameConventionSuffix, StringComparison.Ordinal))
                : $"Unknown{EntityTypeNameConventionSuffix}";
        }

        internal void AddExpression<TValue>(Combine combine, string columnName, Condition condition, TValue value)
        {
            this.expressions.Add(new QueryExpression
            {
                Combiner = combine,
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = condition,
                    Value = value
                }
            });
        }

        internal void AddExpression(Combine combine, List<QueryExpression> nestedExpressions)
        {
            this.expressions.Add(new QueryExpression
            {
                Combiner = combine,
                NestedExpressions = nestedExpressions
            });
        }
    }
}