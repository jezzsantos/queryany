using System;

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

        public DateTime CreatedAtUtc { get; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public DateTime? LastPersistedAtUtc { get; }

        public bool? IsDeleted { get; }

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

        protected abstract void OnEventRaised(IChangeEvent @event);

        protected void RaiseChangeEvent(IChangeEvent @event)
        {
            ((IPublishingEntity) this).RaiseEvent(@event);
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