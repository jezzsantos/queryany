using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.Azure
{
    public abstract class AzureStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity
    {
        private readonly IAzureStorageConnection connection;
        private readonly ILogger logger;

        protected AzureStorage(ILogger logger, EntityFactory<TEntity> entityFactory, IAzureStorageConnection connection)
        {
            logger.GuardAgainstNull(nameof(logger));
            connection.GuardAgainstNull(nameof(connection));
            entityFactory.GuardAgainstNull(nameof(entityFactory));
            this.logger = logger;
            this.connection = connection;
            EntityFactory = entityFactory;
        }

        protected abstract string ContainerName { get; }

        public EntityFactory<TEntity> EntityFactory { get; }

        public Identifier Add(TEntity entity)
        {
            using (var repository = this.connection.Open())
            {
                var id = repository.Add(ContainerName, entity);
                this.logger.LogDebug("Entity {Id} was added to repository", id);
                return id;
            }
        }

        public void Delete(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            using (var repository = this.connection.Open())
            {
                repository.Remove<TEntity>(ContainerName, id);
                this.logger.LogDebug("Entity {Id} was deleted from repository", id);
            }
        }

        public TEntity Get(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            using (var repository = this.connection.Open())
            {
                var entity = repository.Retrieve(ContainerName, id, EntityFactory);
                this.logger.LogDebug("Entity {Id} was retrieved from repository", id);
                return entity;
            }
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

            using (var repository = this.connection.Open())
            {
                var updated = repository.Replace(ContainerName, entity.Id, latest, EntityFactory);
                this.logger.LogDebug("Entity {Id} was updated in repository", entity.Id);
                return updated;
            }
        }

        public long Count()
        {
            using (var repository = this.connection.Open())
            {
                return repository.Count(ContainerName);
            }
        }

        public void DestroyAll()
        {
            using (var repository = this.connection.Open())
            {
                repository.DestroyAll(ContainerName);
                this.logger.LogDebug("All entities were deleted from repository");
            }
        }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options)
        {
            query.GuardAgainstNull(nameof(query));

            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository");
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            List<TEntity> resultEntities;
            using (var repository = this.connection.Open())
            {
                resultEntities = repository.Query(ContainerName, query, EntityFactory);
            }

            this.logger.LogDebug("Entities were retrieved from repository");
            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}