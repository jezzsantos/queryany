using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;
using QueryAny.Properties;

namespace QueryAny
{
    public class QueryClause<TPrimaryEntity> where TPrimaryEntity : IQueryableEntity
    {
        private readonly QueriedEntities entities;

        public QueryClause(QueriedEntities entities)
        {
            entities.GuardAgainstNull(nameof(entities));
            this.entities = entities;
        }

        public IReadOnlyList<QueriedEntity> AllEntities => this.entities.AllEntities;

        public QueriedEntity PrimaryEntity =>
            this.entities.PrimaryEntity;

        public IReadOnlyList<QueriedEntity> JoinedEntities => this.entities.JoinedEntities;

        public IReadOnlyList<WhereExpression> Wheres => this.entities.Wheres;

        public QueryOptions Options => this.entities.Options;

        public ResultOptions ResultOptions => this.entities.ResultOptions;

        public QueryClause<TPrimaryEntity> AndWhere<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            propertyName.GuardAgainstNull(nameof(propertyName));
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'AndWhere' before a 'Where', or after a 'WhereAll'");
            }

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            this.entities.AddWhere(LogicalOperator.And, fieldName, condition, value);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> OrWhere<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            propertyName.GuardAgainstNull(nameof(propertyName));
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'OrWhere' before a 'Where', or after a 'WhereAll'");
            }

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            this.entities.AddWhere(LogicalOperator.Or, fieldName, condition, value);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> AndWhere(
            Func<FromClause<TPrimaryEntity>, QueryClause<TPrimaryEntity>> subWhere)
        {
            subWhere.GuardAgainstNull(nameof(subWhere));
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'AndWhere' before a 'Where', or after a 'WhereAll'");
            }

            this.entities.AddCondition(LogicalOperator.And, subWhere);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> OrWhere(
            Func<FromClause<TPrimaryEntity>, QueryClause<TPrimaryEntity>> subWhere)
        {
            subWhere.GuardAgainstNull(nameof(subWhere));

            bool AnyWhereDefined()
            {
                return this.entities.Wheres.Any();
            }

            if (!AnyWhereDefined())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'OrWhere' before a 'Where', or after a 'WhereAll'");
            }

            this.entities.AddCondition(LogicalOperator.Or, subWhere);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> Select<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName)
        {
            propertyName.GuardAgainstNull(nameof(propertyName));

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            PrimaryEntity.AddSelected(fieldName);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> SelectFromJoin<TJoiningEntity, TValue>(
            Expression<Func<TPrimaryEntity, TValue>> fromEntityPropertyName,
            Expression<Func<TJoiningEntity, TValue>> joiningEntityPropertyName)
            where TJoiningEntity : IQueryableEntity
        {
            fromEntityPropertyName.GuardAgainstNull(nameof(fromEntityPropertyName));
            joiningEntityPropertyName.GuardAgainstNull(nameof(joiningEntityPropertyName));
            var joinedFieldName = Reflector<TPrimaryEntity>.GetPropertyName(fromEntityPropertyName);
            var joiningEntityName = typeof(TJoiningEntity).GetEntityNameSafe();
            var joiningFieldName = Reflector<TJoiningEntity>.GetPropertyName(joiningEntityPropertyName);

            bool IsAnyJoinsDefined()
            {
                return this.entities.JoinedEntities.Any(e => e.Join != null);
            }

            bool IsJoinDefined(string entityName)
            {
                return this.entities.JoinedEntities.Any(e => e.EntityName.EqualsOrdinal(entityName) && e.Join != null);
            }

            if (!IsAnyJoinsDefined())
            {
                throw new InvalidOperationException(
                    Resources.QueryClause_SelectFromJoin_NoJoins.Format(joiningEntityName));
            }

            if (!IsJoinDefined(joiningEntityName))
            {
                throw new InvalidOperationException(
                    Resources.QueryClause_SelectFromJoin_UnknownJoin.Format(joiningEntityName));
            }

            var joiningEntity = JoinedEntities.First(e => e.EntityName.EqualsOrdinal(joiningEntityName));
            var joinedEntity = PrimaryEntity.EntityName;
            joiningEntity.AddSelected(joiningFieldName, joinedEntity, joinedFieldName);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> Take(long limit)
        {
            this.entities.SetLimit(limit);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> Skip(long offset)
        {
            this.entities.SetOffset(offset);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> OrderBy(Expression<Func<TPrimaryEntity, string>> by,
            OrderDirection direction = OrderDirection.Ascending)
        {
            this.entities.SetOrdering(by, direction);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }
    }
}