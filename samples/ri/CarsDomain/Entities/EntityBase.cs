using System;
using System.Collections.Generic;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
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

        public Identifier Id { get; private set; }

        public abstract string EntityName { get; }

        public void Identify(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));
            Id = id;
        }

        public virtual Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id.Dehydrate()},
                {nameof(CreatedAtUtc), CreatedAtUtc},
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc}
            };
        }

        public virtual void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            Id = Identifier.Create(properties.GetValueOrDefault<string>(nameof(Id)));
            CreatedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedAtUtc));
            LastModifiedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(LastModifiedAtUtc));
        }
    }
}