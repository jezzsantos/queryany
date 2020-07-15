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
    public abstract class AzureStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        private readonly IAzureStorageConnection connection;
        private readonly ILogger logger;

        protected AzureStorage(ILogger logger, IAzureStorageConnection connection)
        {
            logger.GuardAgainstNull(nameof(logger));
            connection.GuardAgainstNull(nameof(connection));
            this.logger = logger;
            this.connection = connection;
        }

        protected abstract string ContainerName { get; }

        public Identifier Add(TEntity entity)
        {
            using (var repository = this.connection.Open())
            {
                var id = repository.Add(ContainerName, entity);
                this.logger.LogDebug("Entity {0} was added to repository", id);
                return id;
            }
        }

        public void Delete(Identifier id, bool ignoreConcurrency)
        {
            id.GuardAgainstNull(nameof(id));

            using (var repository = this.connection.Open())
            {
                repository.Remove<TEntity>(ContainerName, id);
                this.logger.LogDebug("Entity {0} was deleted from repository", id);
            }
        }

        public TEntity Get(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            using (var repository = this.connection.Open())
            {
                var entity = repository.Retrieve<TEntity>(ContainerName, id);
                this.logger.LogDebug("Entity {0} was retrieved from repository", id);
                return entity;
            }
        }

        public TEntity Update(TEntity entity, bool ignoreConcurrency)
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
                var updated = repository.Replace(ContainerName, entity.Id, latest);
                this.logger.LogDebug("Entity {0} was updated in repository", entity.Id.Get());
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
                resultEntities = repository.Query(ContainerName, query);
            }

            this.logger.LogDebug("Entities were retrieved from repository");
            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}