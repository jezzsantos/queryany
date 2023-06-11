using System;
using QueryAny.Extensions;

namespace QueryAny
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EntityNameAttribute : Attribute
    {
        public EntityNameAttribute(string entityName)
        {
            entityName.GuardAgainstNullOrEmpty(nameof(entityName));
            EntityName = entityName;
        }

        public string EntityName { get; set; }
    }
}