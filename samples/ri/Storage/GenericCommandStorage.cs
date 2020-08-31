using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    /// <summary>
    ///     Storage for either eventing or traditional storage for commands
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class GenericCommandStorage<TEntity> : ICommandStorage<TEntity> where TEntity : IPersistableEntity
    {
        private readonly ILogger logger;
        private readonly IRepository repository;

        protected GenericCommandStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.logger = logger;
            this.repository = repository;
            DomainFactory = domainFactory;
        }

        protected virtual string ContainerName => typeof(TEntity).GetEntityNameSafe();

        public IDomainFactory DomainFactory { get; }

        public TAggregateRoot Load<TAggregateRoot>(Identifier id) where TAggregateRoot : IPersistableAggregateRoot
        {
            id.GuardAgainstNull(nameof(id));

            var streamName = GetEventStreamName(id);
            var containerName = GetEventContainerName();

            var events = this.repository.Query(containerName,
                Query.From<EventEntity>()
                    .Where(ee => ee.StreamName, ConditionOperator.EqualTo, streamName)
                    .OrderBy(ee => ee.LastPersistedAtUtc), DomainFactory);
            if (!events.Any())
            {
                return RehydrateAggregateRoot<TAggregateRoot>(id, null);
            }

            var lastPersistedAtUtc = events.Last().LastPersistedAtUtc;
            var aggregate = RehydrateAggregateRoot<TAggregateRoot>(id, lastPersistedAtUtc);
            aggregate.LoadChanges(events);

            return aggregate;
        }

        public void Save<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : IPersistableAggregateRoot
        {
            aggregate.GuardAgainstNull(nameof(aggregate));

            if (!aggregate.Id.HasValue() || aggregate.Id.IsEmpty())
            {
                throw new ResourceConflictException("The aggregate does not have an Identifier");
            }

            var events = aggregate.GetChanges();
            if (!events.Any())
            {
                return;
            }

            var containerName = GetEventContainerName();

            events.ForEach(change =>
                this.repository.Add(containerName, change));

            if (OnEventStreamStateChanged != null)
            {
                var streamName = events.First().StreamName;
                var changes = events
                    .Select(ToStateChange)
                    .ToList();
                try
                {
                    OnEventStreamStateChanged.Invoke(this, new EventStreamStateChangedArgs(streamName, changes));
                }
                catch (Exception ex)
                {
                    //Ignore exception and continue
                    this.logger.LogError(ex, $"Handling of event stream failed. Error was: {ex}");
                }
            }

            aggregate.ClearChanges();
        }

        public event EventStreamStateChanged OnEventStreamStateChanged;

        public void Delete(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            this.repository.Remove<TEntity>(ContainerName, id);
            this.logger.LogDebug("Entity {Id} was deleted from repository", id);
        }

        public TEntity Get(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            var entity = this.repository.Retrieve<TEntity>(ContainerName, id, DomainFactory);

            this.logger.LogDebug("Entity {Id} was retrieved from repository", id);

            return entity;
        }

        public TEntity Upsert(TEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));
            if (!entity.Id.HasValue() || entity.Id.IsEmpty())
            {
                throw new ResourceNotFoundException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                this.repository.Add(ContainerName, entity);

                this.logger.LogDebug("Entity {Id} was added to repository", entity.Id);

                return this.repository.Retrieve<TEntity>(ContainerName, entity.Id, DomainFactory);
            }

            latest.PopulateWithNonDefaultValues(entity);

            var updated = this.repository.Replace(ContainerName, entity.Id, latest, DomainFactory);

            this.logger.LogDebug("Entity {Id} was updated in repository", entity.Id);

            return updated;
        }

        public long Count()
        {
            return this.repository.Count(ContainerName);
        }

        public void DestroyAll()
        {
            this.repository.DestroyAll(ContainerName);
            this.logger.LogDebug("All entities were deleted from repository");
        }

        private string GetEventStreamName(Identifier id)
        {
            return $"{ContainerName}_{id}";
        }

        private string GetEventContainerName()
        {
            return $"{ContainerName}_Events";
        }

        private static EventStreamStateChangeEvent ToStateChange(EventEntity @event)
        {
            var change = @event.ConvertTo<EventStreamStateChangeEvent>();
            change.Id = @event.Id;
            change.Type = @event.TypeName;
            change.Version = @event.Version;
            return change;
        }

        private TAggregateRoot RehydrateAggregateRoot<TAggregateRoot>(Identifier id, DateTime? lastPersistedAtUtc)
            where TAggregateRoot : IPersistableAggregateRoot
        {
            return (TAggregateRoot) DomainFactory.RehydrateEntity(typeof(TAggregateRoot), new Dictionary<string, object>
            {
                {nameof(IPersistableEntity.Id), id},
                {nameof(IPersistableEntity.LastPersistedAtUtc), lastPersistedAtUtc}
            });
        }
    }
}