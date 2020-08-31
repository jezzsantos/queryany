using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface ICommandStorage<TEntity> : IEventingCommandStorage, ISnapshotCommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        IDomainFactory DomainFactory { get; }

        long Count();

        void DestroyAll();
    }
}