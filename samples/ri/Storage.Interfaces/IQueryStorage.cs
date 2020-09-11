using QueryAny;

namespace Storage.Interfaces
{
    public interface IQueryStorage<TDto> where TDto : IQueryableEntity, new()
    {
        QueryResults<TDto> Query(QueryClause<TDto> query);

        long Count();

        void DestroyAll();
    }
}