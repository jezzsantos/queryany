using System.Collections.Generic;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public abstract class InProcessInMemStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        private readonly InProcessInMemRepository store;

        protected InProcessInMemStorage(InProcessInMemRepository store)
        {
            store.GuardAgainstNull(nameof(store));

            this.store = store;
        }

        protected abstract string ContainerName { get; }

        public Identifier Add(TEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));
            return this.store.Add(ContainerName, entity);
        }

        public void Delete(Identifier id, bool ignoreConcurrency)
        {
            id.GuardAgainstNull(nameof(id));

            this.store.Remove<TEntity>(ContainerName, id);
        }

        public TEntity Get(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            return this.store.Retrieve<TEntity>(ContainerName, id);
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

            return this.store.Replace(ContainerName, entity.Id, latest);
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
            query.GuardAgainstNull(nameof(query));

            if (query == null || query.Options.IsEmpty)
            {
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var resultEntities = this.store.Query(ContainerName, query);

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}