using Domain.Interfaces.Entities;

namespace Application.Storage.Interfaces
{
    public interface IEventStreamStorage<TAggregateRoot> : IEventNotifyingStorage
        where TAggregateRoot : IPersistableAggregateRoot
    {
        TAggregateRoot Load(Identifier id);

        void Save(TAggregateRoot aggregate);

        void DestroyAll();
    }
}