using System.Collections.Generic;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.Redis
{
    public abstract class RedisInMemStorage<TEntity> : IStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        private readonly IRepository repository;

        protected RedisInMemStorage(RedisInMemRepository repository)
        {
            Guard.AgainstNull(() => repository, repository);
            this.repository = repository;
        }

        protected abstract string ContainerName { get; }

        public string Add(TEntity entity)
        {
            return this.repository.Add(ContainerName, entity);
        }

        public void Delete(string id, bool ignoreConcurrency)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            this.repository.Remove<TEntity>(ContainerName, id);
        }

        public TEntity Get(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            return this.repository.Retrieve<TEntity>(ContainerName, id);
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

            latest.PopulateWithNonDefaultValues(entity);

            return this.repository.Replace(ContainerName, entity.Id, latest);
        }

        public long Count()
        {
            return this.repository.Count(ContainerName);
        }

        public void DestroyAll()
        {
            this.repository.DestroyAll(ContainerName);
        }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options)
        {
            Guard.AgainstNull(() => query, query);

            if (query == null || query.Options.IsEmpty)
            {
                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var resultEntities = this.repository.Query(ContainerName, query);

            return new QueryResults<TEntity>(resultEntities.ConvertAll(e => e));
        }
    }
}