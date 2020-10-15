﻿using System;
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
        private Action<IChangeEvent> aggregateEntityEventHandler;

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

        public DateTime CreatedAtUtc { get; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public DateTime? LastPersistedAtUtc { get; }

        public Identifier Id { get; }

        void IPublishedEntityEventHandler.HandleEvent(IChangeEvent @event)
        {
            OnEventRaised(@event);
        }

        void IPublishingEntity.RaiseEvent(IChangeEvent @event)
        {
            OnEventRaised(@event);
            this.aggregateEntityEventHandler?.Invoke(@event);
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        public void SetAggregateEventHandler(Action<IChangeEvent> handler)
        {
            this.aggregateEntityEventHandler = handler;
        }

        protected abstract void OnEventRaised(IChangeEvent @event);

        protected void RaiseChangeEvent(IChangeEvent @event)
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