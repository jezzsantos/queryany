using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using QueryAny;

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
        private readonly List<IChangeEvent> events;
        private readonly bool isInstantiating;

        protected AggregateRootBase(IRecorder recorder, IIdentifierFactory idFactory,
            Func<Identifier, IChangeEvent> createdEventFactory) : this(recorder, idFactory,
            Identifier.Empty())
        {
            createdEventFactory.GuardAgainstNull(nameof(createdEventFactory));

            Id = idFactory.Create(this);
            RaiseCreateEvent(createdEventFactory(Id));
        }

        /// <summary>
        ///     Creates a new instance of the aggregate with the specified <see cref="Identifier" />,
        ///     used during persistence instantiation. Does not raise any create event.
        /// </summary>
        protected AggregateRootBase(IRecorder recorder, IIdentifierFactory idFactory, Identifier identifier)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            idFactory.GuardAgainstNull(nameof(idFactory));
            identifier.GuardAgainstNull(nameof(identifier));
            Recorder = recorder;
            IdFactory = idFactory;
            Id = identifier;
            this.events = new List<IChangeEvent>();
            this.isInstantiating = identifier == Identifier.Empty();

            var now = DateTime.UtcNow;
            LastPersistedAtUtc = null;
            IsDeleted = null;
            CreatedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            LastModifiedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            ChangeVersion = 0;
        }

        protected IRecorder Recorder { get; }

        protected IIdentifierFactory IdFactory { get; }

        public IReadOnlyList<object> Events => this.events;

        public long ChangeVersion { get; private set; }

        public DateTime CreatedAtUtc { get; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

        public bool? IsDeleted { get; }

        public Identifier Id { get; }

        void IPublishedEntityEventHandler.HandleEvent(IChangeEvent @event)
        {
            OnStateChanged(@event);
        }

        public List<EntityEvent> GetChanges()
        {
            var entityName = GetType().GetEntityNameSafe();
            var streamName = $"{entityName}_{Id}";
            var version = ChangeVersion;
            return this.events.Select(e =>
            {
                var entity = new EntityEvent();
                entity.SetIdentifier(IdFactory);
                entity.SetEvent(streamName, GetType().Name, ++version, e);
                return entity;
            }).ToList();
        }

        public void ClearChanges()
        {
            LastPersistedAtUtc = DateTime.UtcNow;
            this.events.Clear();
        }

        void IPersistableAggregateRoot.OnStateChanged(IChangeEvent @event)
        {
            OnStateChanged(@event);
        }

        void IPersistableAggregateRoot.LoadChanges(IEnumerable<EntityEvent> history, IChangeEventMigrator migrator)
        {
            foreach (var item in history)
            {
                var @event = item.ToEvent(migrator);
                OnStateChanged(@event);

                var expectedVersion = ChangeVersion + 1;
                if (item.Version != expectedVersion)
                {
                    throw new InvalidOperationException(
                        $"The version of this loaded change ('{item.Version}') was not expected. Expected version '{expectedVersion}'. Perhaps a missing change or this change was replayed out of order?");
                }
                ChangeVersion = expectedVersion;
            }
        }

        void IPublishingEntity.RaiseEvent(IChangeEvent @event, bool validate)
        {
            OnStateChanged(@event);
            if (validate)
            {
                var isValid = EnsureValidState();
                if (!isValid)
                {
                    throw new RuleViolationException($"The entity with {Id} is in an invalid state.");
                }
            }
            else
            {
                EnsureBaseValidState();
            }
            LastModifiedAtUtc = DateTime.UtcNow;
            this.events.Add(@event);
        }

        protected abstract void OnStateChanged(IChangeEvent @event);

        private void RaiseCreateEvent(IChangeEvent @event)
        {
            if (this.isInstantiating)
            {
                ((IPublishingEntity) this).RaiseEvent(@event, false);
            }
        }

        protected void RaiseChangeEvent(IChangeEvent @event)
        {
            ((IPublishingEntity) this).RaiseEvent(@event, true);
        }

        protected virtual bool EnsureValidState()
        {
            return EnsureBaseValidState();
        }

        private bool EnsureBaseValidState()
        {
            if (!Id.HasValue())
            {
                throw new RuleViolationException("The entity has no identifier.");
            }

            return true;
        }

        protected static void RaiseToEntity(IPublishedEntityEventHandler entity, IChangeEvent @event)
        {
            @event.EntityId = entity.Id;
            entity.HandleEvent(@event);
        }
    }
}