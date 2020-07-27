using System;
using System.Reflection;

namespace QueryAny
{
    public static class QueryExtensions
    {
        private const string EntityTypeNameConventionSuffix = @"Entity";

        public static string GetEntityNameSafe(this Type entityType)
        {
            var entityNameAttribute = entityType.GetCustomAttribute<EntityNameAttribute>();
            if (entityNameAttribute != null)
            {
                return entityNameAttribute.EntityName;
            }

            var name = entityType.Name;

            return name.EndsWith(EntityTypeNameConventionSuffix)
                ? name.Substring(0, name.LastIndexOf(EntityTypeNameConventionSuffix, StringComparison.Ordinal))
                : $"{name.ToLowerInvariant()}";
        }
    }
}