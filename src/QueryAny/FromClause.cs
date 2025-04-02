using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using QueryAny.Extensions;
using QueryAny.Properties;

namespace QueryAny
{
    public class FromClause<TPrimaryEntity>
        where TPrimaryEntity : IQueryableEntity
    {
        private readonly QueriedEntities entities = new([new QueriedEntity(typeof(TPrimaryEntity))]);

        public QueriedEntity PrimaryEntity => this.entities.PrimaryEntity;

        public IReadOnlyList<WhereExpression> Wheres => this.entities.Wheres;

        public ResultOptions ResultOptions => this.entities.ResultOptions;

        public QueryClause<TPrimaryEntity> Where<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition, TValue value)
        {
            return Where(this.entities, propertyName, condition, value);
        }

        public QueryClause<TPrimaryEntity> Where<TValue>(Expression<Func<TPrimaryEntity, TValue>> propertyName,
            ConditionOperator condition, TValue[] value)
        {
            return Where(this.entities, propertyName, condition, value);
        }

        public QueryClause<TPrimaryEntity> WhereNoOp()
        {
            return WhereNoOp(this.entities);
        }

        public QueryClause<TPrimaryEntity> WhereAll()
        {
            return WhereAll(this.entities);
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

        internal static QueryClause<TPrimaryEntity> Where<TValue>(QueriedEntities entities,
            Expression<Func<TPrimaryEntity, TValue>> propertyName, ConditionOperator condition, object value)
        {
            propertyName.GuardAgainstNull(nameof(propertyName));

            if (entities.Options.Wheres != WhereOptions.Undefined)
            {
                throw new InvalidOperationException(Resources.FromClause_WhereAndNotEmpty);
            }

            var fieldName = Reflector<TPrimaryEntity>.GetPropertyName(propertyName);
            entities.Where(LogicalOperator.None, fieldName, condition, value);

            return new QueryClause<TPrimaryEntity>(entities);
        }

        internal static QueryClause<TPrimaryEntity> WhereNoOp(QueriedEntities entities)
        {
            if (entities.Options.Wheres != WhereOptions.Undefined)
            {
                throw new InvalidOperationException(Resources.FromClause_WhereNoOpAndNotEmpty);
            }

            entities.Options.Wheres = WhereOptions.SomeDefined;

            return new QueryClause<TPrimaryEntity>(entities);
        }

        internal static QueryClause<TPrimaryEntity> WhereAll(QueriedEntities entities)
        {
            if (entities.Options.Wheres != WhereOptions.Undefined)
            {
                throw new InvalidOperationException(Resources.FromClause_WhereAllAndNotEmpty);
            }

            entities.Options.Wheres = WhereOptions.AllDefined;

            return new QueryClause<TPrimaryEntity>(entities);
        }
    }
}