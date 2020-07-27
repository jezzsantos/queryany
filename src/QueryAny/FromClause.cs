using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;

namespace QueryAny
{
    public class FromClause<TPrimaryEntity>
        where TPrimaryEntity : IQueryableEntity
    {
        private readonly QueriedEntities entities;

        public FromClause()
        {
            this.entities = new QueriedEntities(new List<QueriedEntity>
            {
                new QueriedEntity(typeof(TPrimaryEntity))
            });
        }

        public QueriedEntity PrimaryEntity => this.entities.PrimaryEntity;

        public IReadOnlyList<WhereExpression> Wheres => this.entities.Wheres;

        public ResultOptions ResultOptions => this.entities.ResultOptions;

        public QueryClause<TPrimaryEntity> Where<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition,
            TValue value)
        {
            propertyName.GuardAgainstNull(nameof(propertyName));

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            this.entities.AddWhere(LogicalOperator.None, fieldName, condition, value);

            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public JoinClause<TPrimaryEntity, TJoiningEntity> Join<TJoiningEntity, TValue>(
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

            return new JoinClause<TPrimaryEntity, TJoiningEntity>(this.entities);
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
    }
}