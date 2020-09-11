using System;

namespace Storage.Interfaces
{
    public interface IReadModelStorage<TDto>
        where TDto : IReadModelEntity, new()
    {
        TDto Create(string id, Action<TDto> action = null);

        TDto Update(string id, Action<TDto> action);
    }
}