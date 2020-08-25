using System.Collections.Generic;
using QueryAny.Primitives;

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

        public static bool HasBeenPersisted(this IPersistableEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));

            return entity.LastPersistedAtUtc.HasValue && entity.LastPersistedAtUtc.Value.HasValue();
        }

        public static bool HasBeenModifiedSinceLastPersisted(this IEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));

            return entity.HasBeenPersisted() && entity.LastModifiedAtUtc > entity.LastPersistedAtUtc;
        }

        public static bool RequiresUpsert(this IEntity entity)
        {
            return !entity.HasBeenPersisted() || entity.HasBeenModifiedSinceLastPersisted();
        }
    }
}