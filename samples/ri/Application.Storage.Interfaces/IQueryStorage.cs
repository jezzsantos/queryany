using Domain.Interfaces.Entities;
using QueryAny;

namespace Application.Storage.Interfaces
{
    public interface IQueryStorage<TDto> where TDto : IQueryableEntity, new()
    {
        QueryResults<TDto> Query(QueryClause<TDto> query, bool includeDeleted = false);

        TDtoWithId Get<TDtoWithId>(Identifier id, bool includeDeleted = false)
            where TDtoWithId : IQueryableEntity, IHasIdentity, new();

        long Count();

        void DestroyAll();
    }
}