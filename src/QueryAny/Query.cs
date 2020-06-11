using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using QueryAny.Primitives;

namespace QueryAny
{
    public static class Query
    {
        public static FromClause<TEntity> From<TEntity>() where TEntity : INamedEntity, new()
        {
            var entityType = new TEntity();
            return new FromClause<TEntity>(entityType);
        }
    }

    public class FromClause<TEntity> where TEntity : INamedEntity, new()
    {
        public FromClause(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            Collection = new Collection(entity);
        }

        public Collection Collection { get; }

        public WhereClause<TEntity> Where(Expression<Func<TEntity, string>> propertyName, Condition @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return WhereInternal(columnName, @operator, value);
        }

        public WhereClause<TEntity> Where(Expression<Func<TEntity, DateTime>> propertyName,
            Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return WhereInternal(columnName, @operator, value);
        }

        private WhereClause<TEntity> WhereInternal(string columnName, Condition @operator,
            object value)
        {
            Collection.AddExpression(Combine.None, columnName, @operator, value);
            return new WhereClause<TEntity>(Collection);
        }
    }

    public class WhereClause<TEntity>
    {
        private readonly Collection collection;

        public WhereClause(Collection collection)
        {
            Guard.AgainstNull(() => collection, collection);
            this.collection = collection;
            Collections = new List<Collection> {this.collection}.AsReadOnly();
        }

        public IReadOnlyList<Collection> Collections { get; }

        public WhereClause<TEntity> AndWhere(Expression<Func<TEntity, string>> propertyName, Condition @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return WhereInternal(Combine.And, columnName, @operator, value);
        }

        public WhereClause<TEntity> AndWhere(Expression<Func<TEntity, DateTime>> propertyName, Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return WhereInternal(Combine.And, columnName, @operator, value);
        }

        public WhereClause<TEntity> OrWhere(Expression<Func<TEntity, string>> propertyName, Condition @operator,
            string value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return WhereInternal(Combine.Or, columnName, @operator, value);
        }

        public WhereClause<TEntity> OrWhere(Expression<Func<TEntity, DateTime>> propertyName, Condition @operator,
            DateTime value)
        {
            var columnName = Reflector<TEntity>.GetPropertyName(propertyName);
            return WhereInternal(Combine.Or, columnName, @operator, value);
        }

        private WhereClause<TEntity> WhereInternal(Combine combine, string columnName, Condition @operator,
            object value)
        {
            this.collection.AddExpression(combine, columnName, @operator, value);
            return new WhereClause<TEntity>(this.collection);
        }
    }

    public class Collection
    {
        private const string EntityTypeNameConventionSuffix = @"Entity";
        private readonly INamedEntity entity;
        private readonly List<QueryExpression> expressions;

        public Collection(INamedEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            this.entity = entity;
            this.expressions = new List<QueryExpression>();
        }

        public string Name => GetEntityName();

        public IReadOnlyList<QueryExpression> Expressions => this.expressions.AsReadOnly();

        private string GetEntityName()
        {
            if (this.entity.Name.HasValue())
            {
                return this.entity.Name;
            }

            var name = this.entity.GetType().Name;
            return name.EndsWith(EntityTypeNameConventionSuffix)
                ? name.Substring(0, name.LastIndexOf(EntityTypeNameConventionSuffix, StringComparison.Ordinal))
                : $"Unknown{EntityTypeNameConventionSuffix}";
        }

        internal void AddExpression(Combine combine, string columnName, Condition @operator, object value)
        {
            this.expressions.Add(new QueryExpression
            {
                Combiner = combine,
                Condition = new QueryCondition
                {
                    Column = columnName,
                    Operator = @operator,
                    Value = value
                }
            });
        }
    }
}