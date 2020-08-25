using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.Interfaces
{
    public interface IStorage<TEntity> where TEntity : IPersistableEntity
    {
        IDomainFactory DomainFactory { get; }

        TEntity Add(TEntity entity);

        TEntity Upsert(TEntity entity);

        void Delete(Identifier id);

        TEntity Get(Identifier id);

        QueryResults<TEntity> Query(QueryClause<TEntity> query);

        long Count();

        void DestroyAll();
    }
}