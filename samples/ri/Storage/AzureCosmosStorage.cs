using System.Collections.Generic;
using System.Linq;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public abstract class AzureCosmosStorage<TEntity> : IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        private readonly IAzureCosmosConnection connection;

        protected AzureCosmosStorage(IAzureCosmosConnection connection)
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
                throw new EntityNotIdentifiedException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                throw new EntityNotExistsException("Entity not found");
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

            // TODO: Joins
            foreach (var queriedEntity in query.Entities.Where(e => e.Join != null))
            {
                foreach (var resultEntity in resultEntities)
                {
                    //TODO: Fetch the first entity that matches the join 
                }
            }

            //TODO: selects, resolve any joins, select only selected fields

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}