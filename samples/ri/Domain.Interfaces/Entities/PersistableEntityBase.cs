using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD entity, which has an identifier.
    ///     Entities are equal when their identities are equal.
    ///     Entities support being persisted.
    /// </summary>
    public abstract class PersistableEntityBase : IPersistableEntity
    {
        protected PersistableEntityBase(ILogger logger, IIdentifierFactory idFactory)
            : this(logger, idFactory, Identifier.Empty())
        {
            Id = idFactory.Create(this);
        }

        protected PersistableEntityBase(Identifier identifier, IDependencyContainer container,
            IReadOnlyDictionary<string, object> properties)
            : this(container.Resolve<ILogger>(), container.Resolve<IIdentifierFactory>(), identifier)
        {
            var id = properties.GetValueOrDefault<Identifier>(nameof(Id));
            Id = id.HasValue()
                ? id
                : null;
            LastPersistedAtUtc = properties.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            IsDeleted = properties.GetValueOrDefault<bool?>(nameof(IsDeleted));
            CreatedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedAtUtc));
            LastModifiedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(LastModifiedAtUtc));
        }

        private PersistableEntityBase(ILogger logger, IIdentifierFactory idFactory, Identifier identifier)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            identifier.GuardAgainstNull(nameof(identifier));
            Logger = logger;
            IdFactory = idFactory;
            Id = identifier;

            var isInstantiating = identifier == Identifier.Empty();
            var now = DateTime.UtcNow;
            LastPersistedAtUtc = null;
            CreatedAtUtc = isInstantiating
                ? now
                : DateTime.MinValue;
            LastModifiedAtUtc = isInstantiating
                ? now
                : DateTime.MinValue;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected ILogger Logger { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected IIdentifierFactory IdFactory { get; }

        public DateTime CreatedAtUtc { get; private protected set; }

        public DateTime LastModifiedAtUtc { get; private protected set; }

        public sealed override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is EntityBase other && Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.HasValue()

                // ReSharper disable once NonReadonlyMemberInGetHashCode
                ? Id.GetHashCode()
                : 0;
        }

        public DateTime? LastPersistedAtUtc { get; private protected set; }

        public bool? IsDeleted { get; private protected set; }

        public Identifier Id { get; private protected set; }

        public virtual Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(LastPersistedAtUtc), LastPersistedAtUtc},
                {nameof(IsDeleted), IsDeleted},
                {nameof(CreatedAtUtc), CreatedAtUtc},
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc}
            };
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

            return entity.Id == Id;
        }
#if TESTINGONLY
        public void Delete()
        {
            IsDeleted = true;
        }

        public void Undelete()
        {
            IsDeleted = false;
        }
#endif
    }
}