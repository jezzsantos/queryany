using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;
using QueryAny.Properties;

namespace QueryAny
{
    public class QueriedEntities
    {
        private readonly List<QueriedEntity> entities;
        private readonly List<WhereExpression> wheres;

        public QueriedEntities(List<QueriedEntity> entities)
        {
            entities.GuardAgainstNull(nameof(entities));
            this.entities = entities;
            this.wheres = new List<WhereExpression>();
            Options = new QueryOptions();
            ResultOptions = new ResultOptions();
        }

        public IReadOnlyList<QueriedEntity> AllEntities => this.entities;

        public QueriedEntity PrimaryEntity => this.entities[0];

        public IReadOnlyList<QueriedEntity> JoinedEntities => this.entities.Skip(1).ToList();

        public IReadOnlyList<WhereExpression> Wheres => this.wheres.AsReadOnly();

        public QueryOptions Options { get; }

        public ResultOptions ResultOptions { get; }

        internal void AddWhere<TValue>(LogicalOperator combine, string fieldName, ConditionOperator condition,
            TValue value)
        {
            fieldName.GuardAgainstNullOrEmpty(nameof(fieldName));
            this.wheres.Add(new WhereExpression
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

        internal void AddCondition<TPrimaryEntity>(LogicalOperator combine,
            Func<FromClause<TPrimaryEntity>, QueryClause<TPrimaryEntity>> subWhere)
            where TPrimaryEntity : IQueryableEntity
        {
            var fromClause = new FromClause<TPrimaryEntity>();
            subWhere(fromClause);
            var subWheres = fromClause.Wheres.ToList();

            this.wheres.Add(new WhereExpression
            {
                Operator = combine,
                NestedWheres = subWheres
            });
        }

        internal void AddJoin<TJoiningEntity>(Type joiningEntity, string fromEntityFieldName,
            string joiningEntityFieldName, JoinType type) where TJoiningEntity : IQueryableEntity
        {
            bool IsEntityAlreadyJoinedAtLeastOnce()
            {
                return this.entities
                    .Any(e => e.EntityName.EqualsIgnoreCase(joiningEntity.GetEntityNameSafe()));
            }

            if (IsEntityAlreadyJoinedAtLeastOnce())
            {
                throw new InvalidOperationException(
                    $"You cannot 'Join' on the same Entity ('{joiningEntity.GetEntityNameSafe()}') twice");
            }

            var joinedEntityCollection = new QueriedEntity(joiningEntity);
            joinedEntityCollection.AddJoin(
                new JoinSide(PrimaryEntity.UnderlyingEntity, PrimaryEntity.EntityName, fromEntityFieldName),
                new JoinSide(typeof(TJoiningEntity), typeof(TJoiningEntity).GetEntityNameSafe(),
                    joiningEntityFieldName), type);
            this.entities.Add(joinedEntityCollection);
        }

        internal void UpdateOptions(bool isEmpty)
        {
            if (isEmpty)
            {
                Options.SetEmpty();
            }
        }

        internal void SetLimit(int limit)
        {
            if (ResultOptions.Limit != ResultOptions.DefaultLimit)
            {
                throw new InvalidOperationException(Resources.QueriedEntities_LimitAlreadySet);
            }

            ResultOptions.SetLimit(limit);
        }

        internal void SetOffset(int offset)
        {
            if (ResultOptions.Offset != ResultOptions.DefaultOffset)
            {
                throw new InvalidOperationException(Resources.QueriedEntities_OffsetAlreadySet);
            }

            ResultOptions.SetOffset(offset);
        }

        internal void SetOrdering<TPrimaryEntity>(Expression<Func<TPrimaryEntity, string>> by, OrderDirection direction)
            where TPrimaryEntity : IQueryableEntity
        {
            if (ResultOptions.Order.By.NotEqualsIgnoreCase(ResultOptions.DefaultOrder))
            {
                throw new InvalidOperationException(Resources.QueriedEntities_OrderAlreadySet);
            }

            var byPropertyName = Reflector<TPrimaryEntity>.IsValidPropertyName(by)
                ? Reflector<TPrimaryEntity>.GetPropertyName(by)
                : null;

            ResultOptions.SetOrdering(byPropertyName, byPropertyName == null
                ? ResultOptions.DefaultOrderDirection
                : direction);
        }
    }
}