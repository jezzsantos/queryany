using System;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;

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
            propertyName.GuardAgainstNull(nameof(propertyName));

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            this.entities.AddWhere(LogicalOperator.None, fieldName, condition, value);
            return new QueryClause<TPrimaryEntity>(this.entities);
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

        public QueryClause<TPrimaryEntity> WhereAll()
        {
            if (this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'WhereAll' after a 'Where'");
            }

            return new QueryClause<TPrimaryEntity>(this.entities);
        }
    }
}