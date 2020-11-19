using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.Interfaces
{
    public interface IQueryStorage<TDto> where TDto : IQueryableEntity, new()
    {
        QueryResults<TDto> Query(QueryClause<TDto> query);

        TDtoWithId Get<TDtoWithId>(Identifier id) where TDtoWithId : IQueryableEntity, IHasIdentity, new();

        long Count();

        void DestroyAll();
    }
}