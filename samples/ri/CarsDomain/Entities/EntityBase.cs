using System;
using System.Collections.Generic;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace CarsDomain.Entities
{
    public abstract class EntityBase : IPersistableEntity
    {
        protected EntityBase()
        {
            CreatedAtUtc = DateTime.UtcNow;
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public string Id { get; private set; }

        public abstract string EntityName { get; }

        public void Identify(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);
            Id = id;
        }

        public virtual Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(CreatedAtUtc), CreatedAtUtc},
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc}
            };
        }

        public virtual void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            Id = properties.GetValueOrDefault<string>(nameof(Id));
            CreatedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedAtUtc));
            LastModifiedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(LastModifiedAtUtc));
        }
    }
}