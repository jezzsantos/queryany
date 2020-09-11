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
        private readonly List<object> events;
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
            ChangeVersion = 0;
        }

        protected ILogger Logger { get; }

        protected IIdentifierFactory IdFactory { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        protected long ChangeVersion { get; private set; }

        public IReadOnlyList<object> Events => this.events;

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
                {nameof(ChangeVersion), ChangeVersion}
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
            ChangeVersion = properties.GetValueOrDefault<long>(nameof(ChangeVersion));
        }

        void IPublishedEntityEventHandler.HandleEvent(object @event)
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
                entity.Id = IdFactory.Create(entity);
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

        void IPersistableAggregateRoot.OnStateChanged(object @event)
        {
            OnStateChanged(@event);
        }

        void IPersistableAggregateRoot.LoadChanges(IEnumerable<EntityEvent> history)
        {
            foreach (var entity in history)
            {
                var @event = entity.ToEvent();
                OnStateChanged(@event);

                var expectedVersion = ChangeVersion + 1;
                if (entity.Version != expectedVersion)
                {
                    throw new InvalidOperationException(
                        $"The version of this loaded change ('{entity.Version}') was not expected. Expected version '{expectedVersion}'. Perhaps a missing change or this change was replayed out of order?");
                }
                ChangeVersion = expectedVersion;
            }
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
            this.events.Add(@event);
        }

        protected abstract void OnStateChanged(object @event);

        private void RaiseCreateEvent(object @event)
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