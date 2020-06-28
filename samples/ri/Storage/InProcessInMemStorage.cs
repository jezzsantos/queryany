using System.Collections.Generic;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public abstract class InProcessInMemStorage<TEntity> : IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        private readonly InProcessInMemRepository store;

        protected InProcessInMemStorage(InProcessInMemRepository store)
        {
            Guard.AgainstNull(() => store, store);

            this.store = store;
        }

        protected abstract string ContainerName { get; }

        public string Add(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            return this.store.Add(ContainerName, entity);
        }

        public void Delete(string id, bool ignoreConcurrency)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            this.store.Remove(ContainerName, id);
        }

        public TEntity Get(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            return (TEntity) this.store.Get(ContainerName, id);
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

            this.store.Update(ContainerName, entity.Id, entity);
            return entity;
        }

        public long Count()
        {
            return this.store.Count(ContainerName);
        }

        public void DestroyAll()
        {
            this.store.DestroyAll(ContainerName);
        }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options)
        {
            Guard.AgainstNull(() => query, query);

            if (query == null || query.Options.IsEmpty)
            {
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var resultEntities = this.store.Query(ContainerName, query);

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}