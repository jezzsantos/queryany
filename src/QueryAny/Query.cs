using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryAny.Primitives;
using QueryAny.Properties;

namespace QueryAny
{
    public static class Query
    {
        public static FromClause<TPrimaryEntity> From<TPrimaryEntity>() where TPrimaryEntity : IQueryableEntity
        {
            return new FromClause<TPrimaryEntity>();
        }

        public static QueryClause<TPrimaryEntity> Empty<TPrimaryEntity>() where TPrimaryEntity : IQueryableEntity
        {
            var entities = new QueriedEntities(new List<QueriedEntity>
            {
                new QueriedEntity(typeof(TPrimaryEntity))
            });
            entities.UpdateOptions(true);
            return new QueryClause<TPrimaryEntity>(entities);
        }
    }

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

        public QueryClause<TPrimaryEntity> Take(long limit)
        {
            this.entities.ResultOptions.SetLimit(limit);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> Skip(long offset)
        {
            this.entities.ResultOptions.SetOffset(offset);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }
    }

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
            this.entities.ResultOptions.SetLimit(limit);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }

        public QueryClause<TPrimaryEntity> Skip(long offset)
        {
            this.entities.ResultOptions.SetOffset(offset);
            return new QueryClause<TPrimaryEntity>(this.entities);
        }
    }

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
    }

    public class QueryOptions
    {
        public QueryOptions()
        {
            IsEmpty = false;
        }

        public bool IsEmpty { get; private set; }

        public void SetEmpty()
        {
            IsEmpty = true;
        }
    }

    public class QueriedEntity
    {
        private readonly List<SelectDefinition> selects;

        public QueriedEntity(Type entityType)
        {
            entityType.GuardAgainstNull(nameof(entityType));
            UnderlyingEntity = entityType;
            this.selects = new List<SelectDefinition>();
            Join = null;
        }

        public string EntityName => UnderlyingEntity.GetEntityNameSafe();

        internal Type UnderlyingEntity { get; }

        public IReadOnlyList<SelectDefinition> Selects => this.selects.AsReadOnly();

        public JoinDefinition Join { get; private set; }

        internal void AddJoin(JoinSide left, JoinSide right, JoinType type)
        {
            Join = new JoinDefinition(left, right, type);
        }

        internal void AddSelected(string fieldName)
        {
            this.selects.Add(new SelectDefinition(EntityName, fieldName, null, null));
        }

        internal void AddSelected(string fieldName, string joinedEntityName, string joinedFieldName)
        {
            this.selects.Add(new SelectDefinition(EntityName, fieldName, joinedEntityName,
                joinedFieldName));
        }
    }

    public class ResultOptions
    {
        public const long DefaultLimit = -1;
        public const long DefaultOffset = 0;

        public ResultOptions()
        {
            Limit = DefaultLimit;
            Offset = DefaultOffset;
        }

        public long Limit { get; private set; }

        public long Offset { get; private set; }

        internal void SetLimit(long limit)
        {
            if (limit < 0)
            {
                throw new InvalidOperationException(Resources.QueryClause_Options_InvalidLimit);
            }

            if (limit >= 0)
            {
                Limit = limit;
            }
        }

        internal void SetOffset(long offset)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(Resources.QueryClause_Options_InvalidOffset);
            }

            if (offset >= 0)
            {
                Offset = offset;
            }
        }
    }

    public class JoinDefinition
    {
        public JoinDefinition(JoinSide left, JoinSide right, JoinType type)
        {
            left.GuardAgainstNull(nameof(left));
            right.GuardAgainstNull(nameof(right));
            Left = left;
            Right = right;
            Type = type;
        }

        public JoinSide Left { get; }

        public JoinSide Right { get; }

        public JoinType Type { get; }
    }

    public class JoinSide
    {
        public JoinSide(Type entityType, string entityName, string joinedFieldName)
        {
            entityType.GuardAgainstNull(nameof(entityType));
            entityName.GuardAgainstNullOrEmpty(nameof(entityName));
            joinedFieldName.GuardAgainstNullOrEmpty(nameof(joinedFieldName));
            EntityType = entityType;
            EntityName = entityName;
            JoinedFieldName = joinedFieldName;
        }

        public string EntityName { get; }

        public string JoinedFieldName { get; }

        public Type EntityType { get; }
    }

    public class SelectDefinition
    {
        public SelectDefinition(string entityName, string fieldName, string joinedEntityName, string joinedFieldName)
        {
            entityName.GuardAgainstNullOrEmpty(nameof(entityName));
            fieldName.GuardAgainstNullOrEmpty(nameof(fieldName));
            EntityName = entityName;
            FieldName = fieldName;
            JoinedEntityName = joinedEntityName;
            JoinedFieldName = joinedFieldName;
        }

        public string EntityName { get; }
        public string FieldName { get; }
        public string JoinedEntityName { get; }
        public string JoinedFieldName { get; }
    }
}