using System;
using System.Collections.Generic;
using QueryAny;
using Services.Interfaces.Entities;
using Storage.Interfaces;

namespace Storage
{
    public interface IRepository : IDisposable
    {
        Identifier Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new();

        void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity, new();

        TEntity Retrieve<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity, new();

        TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity)
            where TEntity : IPersistableEntity, new();

        long Count(string containerName);

        List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new();

        void DestroyAll(string containerName);
    }
}