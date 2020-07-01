using QueryAny;
using Services.Interfaces;

namespace Storage.Interfaces
{
    public interface IStorage<TEntity> where TEntity : IPersistableEntity, new()
    {
        string Add(TEntity entity);

        TEntity Update(TEntity entity, bool ignoreConcurrency);

        void Delete(string id, bool ignoreConcurrency);

        TEntity Get(string id);

        QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options);

        long Count();

        void DestroyAll();
    }
}