using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD aggregate root, which has an identifier.
    ///     Aggregates are equal when their identities are equal.
    ///     Aggregates support being persisted, and are loaded and saved with eventing.
    ///     Aggregates operate on all child entities or value objects by raising change events for them and itself.
    /// </summary>
    public abstract class AggregateRootBase : IAggregateRootEntity
    {
        private readonly bool isInstantiating;

        protected AggregateRootBase(ILogger logger, IIdentifierFactory idFactory) : this(logger, idFactory,
            Identifier.Empty())

        {
            Id = idFactory.Create(this);
            RaiseCreateEvent(Entities.Events.Any.Created.Create(Id));
        }

        protected AggregateRootBase(ILogger logger, IIdentifierFactory idFactory, Identifier identifier)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            identifier.GuardAgainstNull(nameof(identifier));
            Logger = logger;
            IdFactory = idFactory;
            Id = identifier;
            this.events = new List<object>();
            this.isInstantiating = identifier == Identifier.Empty();

            var now = DateTime.UtcNow;
            LastPersistedAtUtc = null;
            CreatedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            LastModifiedAtUtc = this.isInstantiating
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
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc},
                {EventsPropertyName, Events}
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
            Events = properties.GetValueOrDefault<List<object>>(EventsPropertyName);
        }

        void IPublishedEntityEventHandler.HandleEvent(object @event)
        {
            OnStateChanged(@event);
        }

        public List<object> Events { get; private set; }

        public void ClearEvents()
        {
            Events.Clear();
        }

        void IPublishingEntity.RaiseEvent(object @event)
        {
            OnStateChanged(@event);
            var isValid = EnsureValidState();
            if (!isValid)
            {
                throw new RuleViolationException($"The entity with {Id} is in an invalid state.");
            }
            LastModifiedAtUtc = DateTime.UtcNow;
            Events.Add(@event);
        }

        protected abstract void OnStateChanged(object @event);

        protected void RaiseCreateEvent(object @event)
        {
            if (this.isInstantiating)
            {
                ((IPublishingEntity) this).RaiseEvent(@event);
            }
        }

        protected void RaiseChangeEvent(object @event)
        {
            ((IPublishingEntity) this).RaiseEvent(@event);
        }

        protected virtual bool EnsureValidState()
        {
            return Id.HasValue();
        }

        protected static void RaiseToEntity(IPublishedEntityEventHandler entity, object @event)
        {
            entity?.HandleEvent(@event);
        }
    }
}