using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public abstract class GenericStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity
    {
        private readonly ILogger logger;
        private readonly IRepository repository;

        protected GenericStorage(ILogger logger, EntityFactory<TEntity> entityFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            entityFactory.GuardAgainstNull(nameof(entityFactory));
            this.logger = logger;
            this.repository = repository;
            EntityFactory = entityFactory;
        }

        protected abstract string ContainerName { get; }

        public EntityFactory<TEntity> EntityFactory { get; }

        public Identifier Add(TEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));
            var id = this.repository.Add(ContainerName, entity);

            this.logger.LogDebug("Entity {Id} was added to repository", id);

            return id;
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

            var entity = this.repository.Retrieve(ContainerName, id, EntityFactory);

            this.logger.LogDebug("Entity {Id} was retrieved from repository", id);

            return entity;
        }

        public TEntity Update(TEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));
            if (!entity.Id.HasValue())
            {
                throw new ResourceNotFoundException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                throw new ResourceNotFoundException();
            }

            latest.PopulateWithNonDefaultValues(entity);

            var updated = this.repository.Replace(ContainerName, entity.Id, latest, EntityFactory);

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

        public QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options)
        {
            query.GuardAgainstNull(nameof(query));

            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository");

                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var resultEntities = this.repository.Query(ContainerName, query, EntityFactory);

            this.logger.LogDebug("Entities were retrieved from repository");

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}