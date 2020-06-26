using System;
using System.Collections.Generic;
using QueryAny;
using Storage.Interfaces;

namespace Storage
{
    public interface IAzureCosmosRepository : IDisposable
    {
        string Add<TEntity>(string containerName, TEntity entity) where TEntity : IKeyedEntity, new();
        void Remove<TEntity>(string containerName, string id) where TEntity : IKeyedEntity, new();
        TEntity Get<TEntity>(string containerName, string id) where TEntity : IKeyedEntity, new();
        void Update<TEntity>(string containerName, string entityId, TEntity entity) where TEntity : IKeyedEntity, new();
        long Count(string containerName);

        List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IKeyedEntity, new();

        void DestroyAll(string containerName);
    }
}