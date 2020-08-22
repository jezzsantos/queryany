using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD entity, which has an identifier.
    ///     Entities are equal when their identities are equal.
    ///     Entities support being persisted
    /// </summary>
    public abstract class EntityBase : IPersistableEntity, IPublishingEntity
    {
        public const string EventsPropertyName = "Events";
        private readonly bool isInstantiating;

        protected EntityBase(ILogger logger, IIdentifierFactory idFactory)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            Logger = logger;
            IdFactory = idFactory;

            var now = DateTime.UtcNow;
            CreatedAtUtc = now;
            LastModifiedAtUtc = now;
            Id = idFactory.Create(this);
            this.isInstantiating = !(idFactory is HydrationIdentifierFactory);
            Events = new List<object>();
        }

        protected ILogger Logger { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected IIdentifierFactory IdFactory { get; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public Identifier Id { get; private set; }

        public virtual Dictionary<string, object> Dehydrate()
        {
            return new Dictionary<string, object>
            {
                {nameof(Id), Id},
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
            CreatedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(CreatedAtUtc));
            LastModifiedAtUtc = properties.GetValueOrDefault<DateTime>(nameof(LastModifiedAtUtc));
            Events = properties.GetValueOrDefault<List<object>>(EventsPropertyName);
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

            Events.Add(@event);
        }

        protected abstract void When(object @event);

        protected virtual bool EnsureValidState()
        {
            return Id.HasValue();
        }

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
            return Id.HasValue()

                // ReSharper disable once NonReadonlyMemberInGetHashCode
                ? Id.GetHashCode()
                : 0;
        }
    }
}