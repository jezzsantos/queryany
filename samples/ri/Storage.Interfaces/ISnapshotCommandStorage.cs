using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface ISnapshotCommandStorage<TEntity> where TEntity : IPersistableEntity
    {
        TEntity Upsert(TEntity entity);

        void Delete(Identifier id);

        TEntity Get(Identifier id);
    }
}