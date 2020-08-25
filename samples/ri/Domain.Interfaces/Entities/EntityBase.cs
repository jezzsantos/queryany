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
    public abstract class EntityBase : IPersistableEntity, IPublishingEntity, IPublishedEntityEventHandler
    {
        private Action<object> aggregateEntityEventHandler;

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
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
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
                {nameof(LastModifiedAtUtc), LastModifiedAtUtc}
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
        }

        void IPublishedEntityEventHandler.HandleEvent(object @event)
        {
            When(@event);
        }

        void IPublishingEntity.RaiseEvent(object @event)
        {
            When(@event);
            this.aggregateEntityEventHandler?.Invoke(@event);
        }

        public void SetAggregateEventHandler(Action<object> handler)
        {
            this.aggregateEntityEventHandler = handler;
        }

        protected abstract void When(object @event);

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