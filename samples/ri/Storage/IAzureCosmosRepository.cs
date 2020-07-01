using System;
using System.Collections.Generic;
using QueryAny;
using Storage.Interfaces;

namespace Storage
{
    public interface IAzureCosmosRepository : IDisposable
    {
        string Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new();
        void Remove<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new();
        TEntity Get<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new();
        void Update<TEntity>(string containerName, string entityId, TEntity entity) where TEntity : IPersistableEntity, new();
        long Count(string containerName);

        List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new();

        void DestroyAll(string containerName);
    }
}