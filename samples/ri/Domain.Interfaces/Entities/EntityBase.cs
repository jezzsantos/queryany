using System;
using Common;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines an DDD entity, which has an identifier.
    ///     Entities are equal when their identities are equal.
    ///     Entities operate on all child entities or value objects by handling raised change events from root aggregates
    /// </summary>
    public abstract class EntityBase : IEntity
    {
        private Action<IChangeEvent> aggregateEntityEventHandler;

        protected EntityBase(IRecorder recorder, IIdentifierFactory idFactory,
            Action<IChangeEvent> aggregateEventHandler) : this(recorder, idFactory)
        {
            SetAggregateEventHandler(aggregateEventHandler);
        }

        protected EntityBase(IRecorder recorder, IIdentifierFactory idFactory)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            idFactory.GuardAgainstNull(nameof(idFactory));
            Recorder = recorder;
            IdFactory = idFactory;
            Id = idFactory.Create(this);

            var now = DateTime.UtcNow;
            LastPersistedAtUtc = null;
            IsDeleted = null;
            CreatedAtUtc = now;
            LastModifiedAtUtc = now;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected IRecorder Recorder { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        protected IIdentifierFactory IdFactory { get; }

        public void SetAggregateEventHandler(Action<IChangeEvent> handler)
        {
            this.aggregateEntityEventHandler = handler;
        }

        public sealed override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is EntityBase other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.HasValue()
                ? Id.GetHashCode()
                : 0;
        }

        public virtual bool EnsureValidState()
        {
            return true;
        }

        public DateTime CreatedAtUtc { get; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public DateTime? LastPersistedAtUtc { get; }

        public bool? IsDeleted { get; }

        public Identifier Id { get; }

        void IPublishedEntityEventHandler.HandleEvent(IChangeEvent @event)
        {
            OnEventRaised(@event);
        }

        void IPublishingEntity.RaiseEvent(IChangeEvent @event, bool validate)
        {
            OnEventRaised(@event);
            if (validate)
            {
                var isValid = EnsureValidState();
                if (!isValid)
                {
                    throw new RuleViolationException($"The entity with {Id} is in an invalid state.");
                }
            }

            this.aggregateEntityEventHandler?.Invoke(@event);
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        protected abstract void OnEventRaised(IChangeEvent @event);

        protected void RaiseChangeEvent(IChangeEvent @event)
        {
            ((IPublishingEntity) this).RaiseEvent(@event, true);
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
    }
}