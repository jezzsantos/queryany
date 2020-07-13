using QueryAny;
using Services.Interfaces;
using Services.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface IStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        Identifier Add(TEntity entity);

        TEntity Update(TEntity entity, bool ignoreConcurrency);

        void Delete(Identifier id, bool ignoreConcurrency);

        TEntity Get(Identifier id);

        QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options);

        long Count();

        void DestroyAll();
    }
}