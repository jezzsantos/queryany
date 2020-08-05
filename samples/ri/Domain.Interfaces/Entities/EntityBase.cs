using System;
using System.Collections.Generic;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD entity, which has an identifier.
    ///     Entities are equal when their identities are equal.
    ///     Entities support being persisted
    /// </summary>
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

        public void Identify(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            if (Id.HasValue())
            {
                throw new RuleViolationException(Properties.Resources.EntityBase_IdentifierExists);
            }
            Id = id;
        }

        public virtual Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id?.Dehydrate()},
                {nameof(CreatedAtUtc), CreatedAtUtc},
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc}
            };
        }

        public virtual void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            var id = properties.GetValueOrDefault<string>(nameof(Id));
            Id = id.HasValue()
                ? Identifier.Create(id)
                : null;
            CreatedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedAtUtc));
            LastModifiedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(LastModifiedAtUtc));
        }

        public sealed override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is EntityBase other && Equals(other);
        }

        private bool Equals(EntityBase entity)
        {
            if (!entity.Id.HasValue())
            {
                return false;
            }

            if (!Id.HasValue())
            {
                return false;
            }

            return entity.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id != null

                // ReSharper disable once NonReadonlyMemberInGetHashCode
                ? Id.GetHashCode()
                : 0;
        }
    }
}