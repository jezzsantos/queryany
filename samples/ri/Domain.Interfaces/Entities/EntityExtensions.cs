using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public static class EntityExtensions
    {
        public static TValue GetValueOrDefault<TValue>(this IReadOnlyDictionary<string, object> properties,
            string propertyName, TValue defaultValue = default)
        {
            if (!properties.ContainsKey(propertyName))
            {
                return defaultValue;
            }

            return (TValue) properties[propertyName];
        }
    }
}