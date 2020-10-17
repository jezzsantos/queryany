using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QueryAny;
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
        private readonly List<IChangeEvent> events;
        private readonly bool isInstantiating;

        protected AggregateRootBase(ILogger logger, IIdentifierFactory idFactory,
            Func<Identifier, IChangeEvent> createdEventFactory) : this(logger, idFactory,
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
        protected AggregateRootBase(ILogger logger, IIdentifierFactory idFactory, Identifier identifier)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            identifier.GuardAgainstNull(nameof(identifier));
            Logger = logger;
            IdFactory = idFactory;
            Id = identifier;
            this.events = new List<IChangeEvent>();
            this.isInstantiating = identifier == Identifier.Empty();

            var now = DateTime.UtcNow;
            LastPersistedAtUtc = null;
            CreatedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            LastModifiedAtUtc = this.isInstantiating
                ? now
                : DateTime.MinValue;
            ChangeVersion = 0;
        }

        protected ILogger Logger { get; }

        protected IIdentifierFactory IdFactory { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        protected long ChangeVersion { get; private set; }

        public IReadOnlyList<object> Events => this.events;

        public DateTime CreatedAtUtc { get; }

        public DateTime LastModifiedAtUtc { get; private set; }

        public DateTime? LastPersistedAtUtc { get; private set; }

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

        int IPersistableAggregateRoot.ChangeVersion
        {
            set => ChangeVersion = value;
        }

        void IPersistableAggregateRoot.OnStateChanged(IChangeEvent @event)
        {
            OnStateChanged(@event);
        }

        void IPersistableAggregateRoot.LoadChanges(IEnumerable<EntityEvent> history)
        {
            foreach (var item in history)
            {
                var @event = item.ToEvent();
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

        void IPublishingEntity.RaiseEvent(IChangeEvent @event)
        {
            OnStateChanged(@event);
            var isValid = EnsureValidState();
            if (!isValid)
            {
                throw new RuleViolationException($"The entity with {Id} is in an invalid state.");
            }
            LastModifiedAtUtc = DateTime.UtcNow;
            this.events.Add(@event);
        }

        protected abstract void OnStateChanged(IChangeEvent @event);

        private void RaiseCreateEvent(IChangeEvent @event)
        {
            if (this.isInstantiating)
            {
                ((IPublishingEntity) this).RaiseEvent(@event);
            }
        }

        protected void RaiseChangeEvent(IChangeEvent @event)
        {
            ((IPublishingEntity) this).RaiseEvent(@event);
        }

        protected virtual bool EnsureValidState()
        {
            return Id.HasValue();
        }

        protected static void RaiseToEntity(IPublishedEntityEventHandler entity, IChangeEvent @event)
        {
            entity?.HandleEvent(@event);
        }
    }
}