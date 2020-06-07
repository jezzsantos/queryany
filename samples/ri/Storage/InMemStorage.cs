using System.Collections.Concurrent;
using System.Collections.Generic;
using QueryAny;
using Services.Interfaces;
using Storage.Interfaces;

namespace CarsApi.Storage
{
    public abstract class InMemStorage<TEntity> : IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        private readonly ConcurrentDictionary<string, TEntity> store = new ConcurrentDictionary<string, TEntity>();

        public void Add(TEntity entity)
        {
            Guard.AgainstNull(() => entity, entity);
            if (!entity.Id.HasValue())
            {
                throw new EntityNotIdentifiedException("Entity has empty identifier");
            }

            var exists = Get(entity.Id) != null;
            if (exists)
            {
                throw new EntityAlreadyExistsException("Entity already exists");
            }

            this.store.TryAdd(entity.Id, entity);
        }

        public void Delete(string id, bool ignoreConcurrency)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            this.store.TryRemove(id, out var found);
        }

        public TEntity Get(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            if (this.store.TryGetValue(id, out var entity))
            {
                return entity;
            }

            return default;
        }

        public TEntity Update(TEntity entity, bool ignoreConcurrency)
        {
            Guard.AgainstNull(() => entity, entity);
            if (!entity.Id.HasValue())
            {
                throw new EntityNotIdentifiedException("Entity has empty identifier");
            }

            var found = Get(entity.Id);
            if (found == null)
            {
                throw new EntityNotExistsException("Entity not found");
            }

            this.store.TryUpdate(entity.Id, entity, found);
            return entity;
        }

        public long Count()
        {
            return this.store.Count;
        }

        public QueryResults<TEntity> Query(Query query, SearchOptions options)
        {
            Guard.AgainstNull(() => query, query);

            if (query.Expressions.Count == 0)
            {
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            // TODO: parse this query and apply to dictionary

            return new QueryResults<TEntity>(new List<TEntity>());
        }
    }
}
