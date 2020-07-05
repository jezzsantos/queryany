using System.Collections.Generic;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.Azure
{
    public abstract class AzureStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        private readonly IAzureStorageConnection connection;

        protected AzureStorage(IAzureStorageConnection connection)
        {
            Guard.AgainstNull(() => connection, connection);
            this.connection = connection;
        }

        protected abstract string ContainerName { get; }

        public string Add(TEntity entity)
        {
            using (var store = this.connection.Open())
            {
                return store.Add(ContainerName, entity);
            }
        }

        public void Delete(string id, bool ignoreConcurrency)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            using (var store = this.connection.Open())
            {
                store.Remove<TEntity>(ContainerName, id);
            }
        }

        public TEntity Get(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            using (var store = this.connection.Open())
            {
                return store.Get<TEntity>(ContainerName, id);
            }
        }

        public TEntity Update(TEntity entity, bool ignoreConcurrency)
        {
            Guard.AgainstNull(() => entity, entity);
            if (!entity.Id.HasValue())
            {
                throw new ResourceNotFoundException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                throw new ResourceNotFoundException();
            }

            latest.PopulateWith(entity);

            using (var store = this.connection.Open())
            {
                store.Update(ContainerName, entity.Id, entity);
            }

            return entity;
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
            Guard.AgainstNull(() => query, query);

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