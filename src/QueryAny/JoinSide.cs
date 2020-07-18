using System;
using QueryAny.Primitives;

namespace QueryAny
{
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
}