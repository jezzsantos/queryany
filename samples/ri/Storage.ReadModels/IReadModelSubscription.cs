using Domain.Interfaces.Entities;

namespace Storage.ReadModels
{
    public interface IReadModelSubscription<TAggregateRoot>
        where TAggregateRoot : IPersistableAggregateRoot
    {
    }
}