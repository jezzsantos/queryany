using System;
using QueryAny.Primitives;

namespace QueryAny
{
    public static class QueryExtensions
    {
        private const string EntityTypeNameConventionSuffix = @"Entity";
        private const string UnknownEntityName = @"Unknown";

        public static string GetEntityNameSafe(this INamedEntity entity)
        {
            if (entity.EntityName.HasValue())
            {
                return entity.EntityName;
            }

            var name = entity.GetType().Name;
            return name.EndsWith(EntityTypeNameConventionSuffix)
                ? name.Substring(0, name.LastIndexOf(EntityTypeNameConventionSuffix, StringComparison.Ordinal))
                : $"{UnknownEntityName}{EntityTypeNameConventionSuffix}";
        }
    }
}