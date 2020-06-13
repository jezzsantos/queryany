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
            var entities = new QueriedEntities(new List<QueriedEntity<INamedEntity>>
            {
                new QueriedEntity<INamedEntity>(entity)
            });
            return new QueryClause<TEntity>(entities);
        }
    }

    public class FromClause<TEntity> where TEntity : INamedEntity, new()
    {
        private readonly QueriedEntities entities;

        public FromClause(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            this.entities = new QueriedEntities(new List<QueriedEntity<INamedEntity>>
            {
                new QueriedEntity<INamedEntity>(entity)
            });
        }

        public IReadOnlyList<QueriedEntity<INamedEntity>> Entities => this.entities.Entities;

        public IReadOnlyList<WhereExpression> Wheres => this.entities.Wheres;

        public QueryClause<TEntity> Where<TValue>(Expression<Func<TEntity, TValue>> propertyExpression,
            ConditionOperator condition,
            TValue value)
        {
            Guard.AgainstNull(() => propertyExpression, propertyExpression);

            var fieldName = Reflector<TEntity>.GetPropertyName(propertyExpression);
            this.entities.AddWhere(LogicalOperator.None, fieldName, condition, value);
            return new QueryClause<TEntity>(this.entities);
        }

        public JoinClause<TEntity> Join<TJoiningEntity, TValue>(
            Expression<Func<TEntity, TValue>> fromEntityPropertyName,
            Expression<Func<TJoiningEntity, TValue>> joiningEntityPropertyName, JoinType type = JoinType.Inner)
            where TJoiningEntity : INamedEntity, new()
        {
            Guard.AgainstNull(() => fromEntityPropertyName, fromEntityPropertyName);
            Guard.AgainstNull(() => joiningEntityPropertyName, joiningEntityPropertyName);
            var fromEntityFieldName = Reflector<TEntity>.GetPropertyName(fromEntityPropertyName);
            var joiningEntityFieldName = Reflector<TJoiningEntity>.GetPropertyName(joiningEntityPropertyName);
            var joiningEntity = new TJoiningEntity();
            this.entities.AddJoin(joiningEntity, fromEntityFieldName, joiningEntityFieldName, type);

            return new JoinClause<TEntity>(this.entities);
        }

        public QueryClause<TEntity> WhereAll()
        {
            return new QueryClause<TEntity>(this.entities);
        }
    }

    public class JoinClause<TJoinedEntity>
        where TJoinedEntity : INamedEntity, new()
    {
        private readonly QueriedEntities entities;

        public JoinClause(QueriedEntities entities)
        {
            Guard.AgainstNull(() => entities, entities);
            this.entities = entities;
        }

        public QueryClause<TJoinedEntity> Where<TValue>(Expression<Func<TJoinedEntity, TValue>> propertyExpression,
            ConditionOperator condition,
            TValue value)
        {
            Guard.AgainstNull(() => propertyExpression, propertyExpression);

            var fieldName = Reflector<TJoinedEntity>.GetPropertyName(propertyExpression);
            this.entities.AddWhere(LogicalOperator.None, fieldName, condition, value);
            return new QueryClause<TJoinedEntity>(this.entities);
        }

        public JoinClause<TJoinedEntity> AndJoin<TJoiningEntity, TValue>(
            Expression<Func<TJoinedEntity, TValue>> fromEntityPropertyName,
            Expression<Func<TJoiningEntity, TValue>> joiningEntityPropertyName, JoinType type = JoinType.Inner)
            where TJoiningEntity : INamedEntity, new()
        {
            Guard.AgainstNull(() => fromEntityPropertyName, fromEntityPropertyName);
            Guard.AgainstNull(() => joiningEntityPropertyName, joiningEntityPropertyName);
            var fromEntityFieldName = Reflector<TJoinedEntity>.GetPropertyName(fromEntityPropertyName);
            var joiningEntityFieldName = Reflector<TJoiningEntity>.GetPropertyName(joiningEntityPropertyName);
            var joiningEntity = new TJoiningEntity();
            this.entities.AddJoin(joiningEntity, fromEntityFieldName, joiningEntityFieldName, type);

            return new JoinClause<TJoinedEntity>(this.entities);
        }
    }

    public class QueryClause<TEntity> where TEntity : INamedEntity, new()
    {
        private readonly QueriedEntities entities;

        public QueryClause(QueriedEntities entities)
        {
            Guard.AgainstNull(() => entities, entities);
            this.entities = entities;
        }

        public IReadOnlyList<QueriedEntity<INamedEntity>> Entities => this.entities.Entities;
        public IReadOnlyList<WhereExpression> Wheres => this.entities.Wheres;

        public QueryClause<TEntity> AndWhere<TValue>(Expression<Func<TEntity, TValue>> propertyExpression,
            ConditionOperator condition,
            TValue value)
        {
            Guard.AgainstNull(() => propertyExpression, propertyExpression);
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'AndWhere' before a 'Where', or after a 'WhereAll'");
            }

            var fieldName = Reflector<TEntity>.GetPropertyName(propertyExpression);
            this.entities.AddWhere(LogicalOperator.And, fieldName, condition, value);
            return new QueryClause<TEntity>(this.entities);
        }

        public QueryClause<TEntity> OrWhere<TValue>(Expression<Func<TEntity, TValue>> propertyExpression,
            ConditionOperator condition,
            TValue value)
        {
            Guard.AgainstNull(() => propertyExpression, propertyExpression);
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'OrWhere' before a 'Where', or after a 'WhereAll'");
            }

            var fieldName = Reflector<TEntity>.GetPropertyName(propertyExpression);
            this.entities.AddWhere(LogicalOperator.Or, fieldName, condition, value);
            return new QueryClause<TEntity>(this.entities);
        }

        public QueryClause<TEntity> AndWhere(Func<FromClause<TEntity>, QueryClause<TEntity>> subWhere)
        {
            Guard.AgainstNull(() => subWhere, subWhere);
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'AndWhere' before a 'Where', or after a 'WhereAll'");
            }

            this.entities.AddCondition(LogicalOperator.And, subWhere);
            return new QueryClause<TEntity>(this.entities);
        }

        public QueryClause<TEntity> OrWhere(Func<FromClause<TEntity>, QueryClause<TEntity>> subWhere)
        {
            Guard.AgainstNull(() => subWhere, subWhere);
            if (!this.entities.Wheres.Any())
            {
                throw new InvalidOperationException(
                    "You cannot use an 'OrWhere' before a 'Where', or after a 'WhereAll'");
            }

            this.entities.AddCondition(LogicalOperator.Or, subWhere);
            return new QueryClause<TEntity>(this.entities);
        }

        public QueryClause<TEntity> Select<TValue>(Expression<Func<TEntity, TValue>> propertyExpression)
        {
            Guard.AgainstNull(() => propertyExpression, propertyExpression);

            var fieldName = Reflector<TEntity>.GetPropertyName(propertyExpression);
            Entities[0].AddSelected(fieldName);
            return new QueryClause<TEntity>(this.entities);
        }
    }

    public class QueriedEntities
    {
        private readonly List<QueriedEntity<INamedEntity>> entities;
        private readonly List<WhereExpression> wheres;

        public QueriedEntities(List<QueriedEntity<INamedEntity>> entities)
        {
            Guard.AgainstNull(() => entities, entities);
            this.entities = entities;
            this.wheres = new List<WhereExpression>();
        }

        public IReadOnlyList<QueriedEntity<INamedEntity>> Entities => this.entities.AsReadOnly();

        public IReadOnlyList<WhereExpression> Wheres => this.wheres.AsReadOnly();

        internal void AddWhere<TValue>(LogicalOperator combine, string fieldName, ConditionOperator condition,
            TValue value)
        {
            Guard.AgainstNullOrEmpty(() => fieldName, fieldName);
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

        internal void AddCondition<TEntity>(LogicalOperator combine,
            Func<FromClause<TEntity>, QueryClause<TEntity>> subWhere) where TEntity : INamedEntity, new()
        {
            var rootEntity = Entities[0];
            var fromClause = new FromClause<TEntity>((TEntity) rootEntity.UnderlyingEntity);
            subWhere(fromClause);
            var subWheres = fromClause.Wheres.ToList();

            this.wheres.Add(new WhereExpression
            {
                Operator = combine,
                NestedWheres = subWheres
            });
        }

        public void AddJoin<TJoiningEntity>(TJoiningEntity joiningEntity, string fromEntityFieldName,
            string joiningEntityFieldName, JoinType type) where TJoiningEntity : INamedEntity, new()
        {
            bool IsEntityAlreadyJoinedAtLeastOnce()
            {
                return this.entities
                    .Any(e => e.UnderlyingEntity.EntityName.EqualsIgnoreCase(joiningEntity.EntityName));
            }

            if (IsEntityAlreadyJoinedAtLeastOnce())
            {
                throw new InvalidOperationException(
                    $"You cannot 'Join' on the same Entity ('{joiningEntity.EntityName}') twice");
            }

            var joinedEntityCollection = new QueriedEntity<INamedEntity>(joiningEntity);
            joinedEntityCollection.AddJoin(
                new JoinSide(Entities[0].Name, fromEntityFieldName),
                new JoinSide(joinedEntityCollection.Name, joiningEntityFieldName), type);
            this.entities.Add(joinedEntityCollection);
        }
    }

    public class QueriedEntity<TEntity> where TEntity : INamedEntity
    {
        private const string EntityTypeNameConventionSuffix = @"Entity";
        private const string UnknownEntityName = @"Unknown";
        private readonly TEntity entity;
        private readonly List<SelectedField> selects;

        public QueriedEntity(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            this.entity = entity;
            this.selects = new List<SelectedField>();
            Join = null;
        }

        public string Name => GetEntityName();

        internal TEntity UnderlyingEntity => this.entity;

        public IReadOnlyList<SelectedField> Selects => this.selects.AsReadOnly();
        public JoinDefinition Join { get; private set; }

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

        internal void AddJoin(JoinSide left, JoinSide right, JoinType type)
        {
            Join = new JoinDefinition(left, right, type);
        }

        public void AddSelected(string fieldName)
        {
            this.selects.Add(new SelectedField(fieldName));
        }
    }

    public class JoinDefinition
    {
        public JoinDefinition(JoinSide left, JoinSide right, JoinType type)
        {
            Guard.AgainstNull(() => left, left);
            Guard.AgainstNull(() => right, right);
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
        public JoinSide(string entityName, string joinedFieldName)
        {
            Guard.AgainstNullOrEmpty(() => entityName, entityName);
            Guard.AgainstNullOrEmpty(() => joinedFieldName, joinedFieldName);
            EntityName = entityName;
            JoinedFieldName = joinedFieldName;
        }

        public string EntityName { get; }
        public string JoinedFieldName { get; }
    }

    public class SelectedField
    {
        public SelectedField(string name)
        {
            Guard.AgainstNullOrEmpty(() => name, name);
            Name = name;
        }

        public string Name { get; }
    }
}