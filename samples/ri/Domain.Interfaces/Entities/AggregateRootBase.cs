using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD aggregate root, which has an identifier.
    ///     Aggregates are equal when their identities are equal.
    ///     Aggregates support being persisted.
    ///     Aggregates operate on all child entities or value objects by raising change events
    /// </summary>
    public abstract class AggregateRootBase : IAggregateRootEntity
    {
        internal const string EventsPropertyName = "Events";
        private readonly bool isInstantiating;

        protected AggregateRootBase(ILogger logger, IIdentifierFactory idFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            Logger = logger;
            IdFactory = idFactory;

            this.isInstantiating = !(idFactory is HydrationIdentifierFactory);
            var now = DateTime.UtcNow;
            LastPersistedAtUtc = null;
            CreatedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            LastModifiedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            Id = idFactory.Create(this);
            Events = new List<object>();
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
            When(@event);
        }

        public List<object> Events { get; private set; }

        public void ClearEvents()
        {
            Events.Clear();
        }

        void IPublishingEntity.RaiseEvent(object @event)
        {
            When(@event);
            var isValid = EnsureValidState();
            if (!isValid)
            {
                throw new RuleViolationException($"The entity with {Id} is in an invalid state.");
            }
            LastModifiedAtUtc = DateTime.UtcNow;
            Events.Add(@event);
        }

        protected abstract void When(object @event);

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

        protected void RaiseToEntity(IPublishedEntityEventHandler entity, object @event)
        {
            entity?.HandleEvent(@event);
        }
    }
}