using QueryAny;

namespace Application.Storage.Interfaces
{
    public interface ICrudStorage<TDto> where TDto : IPersistableDto, new()
    {
        TDto Upsert(TDto dto, bool includeDeleted = false);

        void Delete(string id, bool destroy = true);

        TDto ResurrectDeleted(string id);

        TDto Get(string id, bool includeDeleted = false);

        QueryResults<TDto> Query(QueryClause<TDto> query, bool includeDeleted = false);

        long Count();

        void DestroyAll();
    }
}