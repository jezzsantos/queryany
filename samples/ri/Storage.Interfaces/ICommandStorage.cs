using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface IEventingCommandStorage
    {
        TAggregateRoot Load<TAggregateRoot>(Identifier id) where TAggregateRoot : IPersistableAggregateRoot;

        void Save<TAggregateRoot>(TAggregateRoot aggregate) where TAggregateRoot : IPersistableAggregateRoot;
    }

    public interface ISnapshotCommandStorage<TEntity> where TEntity : IPersistableEntity
    {
        TEntity Upsert(TEntity entity);

        void Delete(Identifier id);

        TEntity Get(Identifier id);
    }

    public interface ICommandStorage<TEntity> : IEventingCommandStorage, ISnapshotCommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        IDomainFactory DomainFactory { get; }

        long Count();

        void DestroyAll();
    }
}