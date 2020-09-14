using System;

namespace Storage.Interfaces.ReadModels
{
    public interface IReadModelStorage<out TDto> where TDto : IReadModelEntity, new()
    {
        TDto Create(string id, Action<TDto> action = null);

        TDto Update(string id, Action<TDto> action);
    }
}