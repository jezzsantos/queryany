using System.Collections.Generic;
using QueryAny;
using Services.Interfaces;
using Services.Interfaces.Entities;

namespace Storage.Interfaces
{
    public delegate TEntity EntityFactory<out TEntity>(IReadOnlyDictionary<string, object> hydratingProperties)
        where TEntity : IPersistableEntity;

    public interface IStorage<TEntity> where TEntity : IPersistableEntity
    {
        EntityFactory<TEntity> EntityFactory { get; }
        
        Identifier Add(TEntity entity);

        TEntity Update(TEntity entity);

        void Delete(Identifier id);

        TEntity Get(Identifier id);

        QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options);

        long Count();

        void DestroyAll();
    }
}