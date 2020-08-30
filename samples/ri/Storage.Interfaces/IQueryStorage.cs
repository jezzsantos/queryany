using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.Interfaces
{
    public interface IQueryStorage<TEntity> where TEntity : IPersistableEntity
    {
        IDomainFactory DomainFactory { get; }

        QueryResults<TEntity> Query(QueryClause<TEntity> query);

        long Count();

        void DestroyAll();
    }
}