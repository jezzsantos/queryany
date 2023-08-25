using System;
using System.Linq.Expressions;
using QueryAny.Extensions;

namespace QueryAny
{
    public class JoinClause<TPrimaryEntity, TJoinedEntity>
        where TPrimaryEntity : IQueryableEntity
        where TJoinedEntity : IQueryableEntity
    {
        private readonly QueriedEntities entities;

        public JoinClause(QueriedEntities entities)
        {
            entities.GuardAgainstNull(nameof(entities));
            this.entities = entities;
        }

        public QueryClause<TPrimaryEntity> Where<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            return FromClause<TPrimaryEntity>.Where(this.entities, propertyName, condition, value);
        }

        public QueryClause<TPrimaryEntity> WhereNoOp()
        {
            return FromClause<TPrimaryEntity>.WhereNoOp(this.entities);
        }

        public QueryClause<TPrimaryEntity> WhereAll()
        {
            return FromClause<TPrimaryEntity>.WhereAll(this.entities);
        }

        public JoinClause<TPrimaryEntity, TJoinedEntity> AndJoin<TJoiningEntity, TValue>(
            Expression<Func<TPrimaryEntity, TValue>> fromEntityPropertyName,
            Expression<Func<TJoiningEntity, TValue>> joiningEntityPropertyName, JoinType type = JoinType.Inner)
            where TJoiningEntity : IQueryableEntity
        {
            fromEntityPropertyName.GuardAgainstNull(nameof(fromEntityPropertyName));
            joiningEntityPropertyName.GuardAgainstNull(nameof(joiningEntityPropertyName));
            var fromEntityFieldName = Reflector<TPrimaryEntity>.GetPropertyName(fromEntityPropertyName);
            var joiningEntityFieldName = Reflector<TJoiningEntity>.GetPropertyName(joiningEntityPropertyName);
            var joiningEntity = typeof(TJoiningEntity);
            this.entities.AddJoin<TJoiningEntity>(joiningEntity, fromEntityFieldName, joiningEntityFieldName, type);

            return new JoinClause<TPrimaryEntity, TJoinedEntity>(this.entities);
        }
    }
}