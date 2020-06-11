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

        public static QueryClause<TEntity> Empty<TEntity>() where TEntity : INamedEntity, new()
        {
            var entity = new TEntity();
            return new QueryClause<TEntity>(new EntityCollection<TEntity>(entity));
        }
    }

    public class FromClause<TEntity> where TEntity : INamedEntity, new()
    {
        public FromClause(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            EntityCollection = new EntityCollection<TEntity>(entity);
        }

        public EntityCollection<TEntity> EntityCollection { get; }

        public QueryClause<TEntity> Where<TValue>(Expression<Func<TEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            var fieldName = Reflector<TEntity>.GetPropertyName(propertyName);
            EntityCollection.AddExpression(LogicalOperator.None, fieldName, condition, value);
            return new QueryClause<TEntity>(EntityCollection);
        }
    }

    public class QueryClause<TEntity> where TEntity : INamedEntity, new()
    {
        private readonly EntityCollection<TEntity> entityCollection;

        public QueryClause(EntityCollection<TEntity> entityCollection)
        {
            Guard.AgainstNull(() => entityCollection, entityCollection);
            this.entityCollection = entityCollection;
            EntityCollections = new List<EntityCollection<TEntity>> {this.entityCollection}.AsReadOnly();
        }

        public IReadOnlyList<EntityCollection<TEntity>> EntityCollections { get; }

        public QueryClause<TEntity> AndWhere<TValue>(Expression<Func<TEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            if (!this.entityCollection.Expressions.Any())
            {
                throw new InvalidOperationException("Must have at least one expression to AND with");
            }

            var fieldName = Reflector<TEntity>.GetPropertyName(propertyName);
            this.entityCollection.AddExpression(LogicalOperator.And, fieldName, condition, value);
            return new QueryClause<TEntity>(this.entityCollection);
        }

        public QueryClause<TEntity> OrWhere<TValue>(Expression<Func<TEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            if (!this.entityCollection.Expressions.Any())
            {
                throw new InvalidOperationException("Must have at least one expression to OR with");
            }

            var fieldName = Reflector<TEntity>.GetPropertyName(propertyName);
            this.entityCollection.AddExpression(LogicalOperator.Or, fieldName, condition, value);
            return new QueryClause<TEntity>(this.entityCollection);
        }

        public QueryClause<TEntity> AndWhere(Func<FromClause<TEntity>, QueryClause<TEntity>> subWhereClause)
        {
            if (!this.entityCollection.Expressions.Any())
            {
                throw new InvalidOperationException("Must have at least one expression to AND with");
            }

            var fromClause = new FromClause<TEntity>(this.entityCollection.UnderlyingEntity);
            subWhereClause(fromClause);

            this.entityCollection.AddExpression(LogicalOperator.And, fromClause.EntityCollection.Expressions.ToList());
            return new QueryClause<TEntity>(this.entityCollection);
        }
    }

    public class EntityCollection<TEntity> where TEntity : INamedEntity, new()
    {
        private const string EntityTypeNameConventionSuffix = @"Entity";
        private const string UnknownEntityName = @"Unknown";
        private readonly TEntity entity;
        private readonly List<WhereExpression> expressions;

        public EntityCollection(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            this.entity = entity;
            this.expressions = new List<WhereExpression>();
        }

        public string Name => GetEntityName();

        internal TEntity UnderlyingEntity => this.entity;

        public IReadOnlyList<WhereExpression> Expressions => this.expressions.AsReadOnly();

        private string GetEntityName()
        {
            if (this.entity.EntityName.HasValue())
            {
                return this.entity.EntityName;
            }

            var name = this.entity.GetType().Name;
            return name.EndsWith(EntityTypeNameConventionSuffix)
                ? name.Substring(0, name.LastIndexOf(EntityTypeNameConventionSuffix, StringComparison.Ordinal))
                : $"{UnknownEntityName}{EntityTypeNameConventionSuffix}";
        }

        internal void AddExpression<TValue>(LogicalOperator combine, string fieldName, ConditionOperator condition,
            TValue value)
        {
            this.expressions.Add(new WhereExpression
            {
                Operator = combine,
                Condition = new WhereCondition
                {
                    FieldName = fieldName,
                    Operator = condition,
                    Value = value
                }
            });
        }

        internal void AddExpression(LogicalOperator combine, List<WhereExpression> nestedExpressions)
        {
            this.expressions.Add(new WhereExpression
            {
                Operator = combine,
                NestedExpressions = nestedExpressions
            });
        }
    }
}