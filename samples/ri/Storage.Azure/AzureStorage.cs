using System.Collections.Generic;
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

        protected AzureStorage(IAzureStorageConnection connection)
        {
            connection.GuardAgainstNull(nameof(connection));
            this.connection = connection;
        }

        protected abstract string ContainerName { get; }

        public Identifier Add(TEntity entity)
        {
            using (var store = this.connection.Open())
            {
                return store.Add(ContainerName, entity);
            }
        }

        public void Delete(Identifier id, bool ignoreConcurrency)
        {
            id.GuardAgainstNull(nameof(id));

            using (var store = this.connection.Open())
            {
                store.Remove<TEntity>(ContainerName, id);
            }
        }

        public TEntity Get(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            using (var store = this.connection.Open())
            {
                return store.Retrieve<TEntity>(ContainerName, id);
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

            using (var store = this.connection.Open())
            {
                return store.Replace(ContainerName, entity.Id, latest);
            }
        }

        public long Count()
        {
            using (var store = this.connection.Open())
            {
                return store.Count(ContainerName);
            }
        }

        public void DestroyAll()
        {
            using (var store = this.connection.Open())
            {
                store.DestroyAll(ContainerName);
            }
        }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options)
        {
            query.GuardAgainstNull(nameof(query));

            if (query == null || query.Options.IsEmpty)
            {
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            List<TEntity> resultEntities;
            using (var store = this.connection.Open())
            {
                resultEntities = store.Query(ContainerName, query);
            }

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}