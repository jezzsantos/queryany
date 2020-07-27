using QueryAny.Primitives;

namespace QueryAny
{
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