using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage
{
    public interface IRepository
    {
        int MaxQueryResults { get; }

        void Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity;

        void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity;

        TEntity Retrieve<TEntity>(string containerName, Identifier id, EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity;

        TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity;

        long Count(string containerName);

        List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity;

        void DestroyAll(string containerName);
    }
}