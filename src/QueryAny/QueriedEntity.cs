using System;
using System.Collections.Generic;
using QueryAny.Primitives;

namespace QueryAny
{
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
}