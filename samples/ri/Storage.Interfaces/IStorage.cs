using QueryAny;
using Services.Interfaces;

namespace Storage.Interfaces
{
    public interface IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        void Add(TEntity entity);

        TEntity Update(TEntity entity, bool ignoreConcurrency);

        void Delete(string id, bool ignoreConcurrency);

        TEntity Get(string id);

        QueryResults<TEntity> Query(AzureCosmosQuery query, SearchOptions options);

        long Count();
    }
}