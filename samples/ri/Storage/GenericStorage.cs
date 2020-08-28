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
    public abstract class GenericStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity
    {
        private readonly ILogger logger;
        private readonly IRepository repository;

        protected GenericStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.logger = logger;
            this.repository = repository;
            DomainFactory = domainFactory;
        }

        protected abstract string ContainerName { get; }

        public IDomainFactory DomainFactory { get; }

        public TAggregateRoot Load<TAggregateRoot>(Identifier id) where TAggregateRoot : IPersistableAggregateRoot
        {
            id.GuardAgainstNull(nameof(id));

            var streamName = GetEventStreamName(id);
            var containerName = GetEventContainerName();

            var events = this.repository.Query(containerName,
                QueryAny.Query.From<EventEntity>()
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

            if (!aggregate.Id.HasValue())
            {
                throw new ResourceConflictException("The aggregate does not have an Identifier");
            }

            var changes = aggregate.GetChanges();
            if (!changes.Any())
            {
                return;
            }

            var containerName = GetEventContainerName();

            changes.ForEach(change =>
                this.repository.Add(containerName, change));

            aggregate.ClearChanges();
        }

        public TEntity Add(TEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));

            if (!entity.Id.HasValue())
            {
                throw new ResourceConflictException("The entity does not have an Identifier");
            }

            this.repository.Add(ContainerName, entity);

            this.logger.LogDebug("Entity {Id} was added to repository", entity.Id);

            return this.repository.Retrieve<TEntity>(ContainerName, entity.Id, DomainFactory);
        }

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
            if (!entity.Id.HasValue())
            {
                throw new ResourceNotFoundException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                return Add(entity);
            }

            latest.PopulateWithNonDefaultValues(entity);

            var updated = this.repository.Replace(ContainerName, entity.Id, latest, DomainFactory);

            this.logger.LogDebug("Entity {Id} was updated in repository", entity.Id);

            return updated;
        }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query)
        {
            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository");

                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var entities = this.repository.Query(ContainerName, query, DomainFactory);

            this.logger.LogDebug("Entities were retrieved from repository");

            return new QueryResults<TEntity>(entities.ConvertAll(e => e));
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