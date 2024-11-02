using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Extensions;
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
            if (!CanAddWhereClauses())
            {
                throw new InvalidOperationException(Resources.QueryClause_AndWhereBeforeWheres);
            }

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            var op = Wheres.Count == 0
                ? LogicalOperator.None
                : LogicalOperator.And;
            this.entities.AddWhere(op, fieldName, condition, value);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> OrWhere<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            propertyName.GuardAgainstNull(nameof(propertyName));
            if (!CanAddWhereClauses())
            {
                throw new InvalidOperationException(Resources.QueryClause_OrWhereBeforeWheres);
            }

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            var op = Wheres.Count == 0
                ? LogicalOperator.None
                : LogicalOperator.Or;
            this.entities.AddWhere(op, fieldName, condition, value);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> AndWhere(
            Func<FromClause<TPrimaryEntity>, QueryClause<TPrimaryEntity>> subWhere)
        {
            subWhere.GuardAgainstNull(nameof(subWhere));
            if (!CanAddWhereClauses())
            {
                throw new InvalidOperationException(Resources.QueryClause_AndWhereBeforeWheres);
            }

            var op = Wheres.Count == 0
                ? LogicalOperator.None
                : LogicalOperator.And;
            this.entities.AddCondition(op, subWhere);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> OrWhere(
            Func<FromClause<TPrimaryEntity>, QueryClause<TPrimaryEntity>> subWhere)
        {
            subWhere.GuardAgainstNull(nameof(subWhere));

            if (!CanAddWhereClauses())
            {
                throw new InvalidOperationException(Resources.QueryClause_OrWhereBeforeWheres);
            }

            var op = Wheres.Count == 0
                ? LogicalOperator.None
                : LogicalOperator.Or;
            this.entities.AddCondition(op, subWhere);

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

        public QueryClause<TPrimaryEntity> Take(int limit)
        {
            this.entities.SetLimit(limit);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> Skip(int offset)
        {
            this.entities.SetOffset(offset);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> OrderBy<TValue>(Expression<Func<TPrimaryEntity, TValue>> by,
            OrderDirection direction = OrderDirection.Ascending)
        {
            this.entities.SetOrdering(by, direction);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        private bool CanAddWhereClauses()
        {
            return this.entities.Options.Wheres == WhereOptions.SomeDefined;
        }
    }
}