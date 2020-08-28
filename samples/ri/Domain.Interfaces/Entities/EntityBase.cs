using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD entity, which has an identifier.
    ///     Entities are equal when their identities are equal.
    ///     Entities support being persisted.
    ///     Entities operate on all child entities or value objects by handling raised change events from root aggregates
    /// </summary>
    public abstract class EntityBase : IEntity
    {
        private Action<object> aggregateEntityEventHandler;

        protected EntityBase(ILogger logger, IIdentifierFactory idFactory) : this(logger, idFactory, Identifier.Empty())
        {
            Id = idFactory.Create(this);
        }

        protected EntityBase(ILogger logger, IIdentifierFactory idFactory, Identifier identifier)
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

        protected ILogger Logger { get; }

        protected IIdentifierFactory IdFactory { get; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

        public Identifier Id { get; private set; }

        public virtual Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
                {nameof(LastPersistedAtUtc), LastPersistedAtUtc},
                {nameof(CreatedAtUtc), CreatedAtUtc},
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc}
            };
        }

        public virtual void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            var id = properties.GetValueOrDefault<Identifier>(nameof(Id));
            Id = id.HasValue()
                ? id
                : null;
            LastPersistedAtUtc = properties.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            CreatedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedAtUtc));
            LastModifiedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(LastModifiedAtUtc));
        }

        void IPublishedEntityEventHandler.HandleEvent(object @event)
        {
            OnEventRaised(@event);
        }

        void IPublishingEntity.RaiseEvent(object @event)
        {
            OnEventRaised(@event);
            this.aggregateEntityEventHandler?.Invoke(@event);
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        public void SetAggregateEventHandler(Action<object> handler)
        {
            this.aggregateEntityEventHandler = handler;
        }

        protected abstract void OnEventRaised(object @event);

        protected void RaiseChangeEvent(object @event)
        {
            ((IPublishingEntity) this).RaiseEvent(@event);
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

            return entity.Id == Id;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id.HasValue()

                // ReSharper disable once NonReadonlyMemberInGetHashCode
                ? Id.GetHashCode()
                : 0;
        }
    }
}