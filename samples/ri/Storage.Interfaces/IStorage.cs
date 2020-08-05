using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.Interfaces
{
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