using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface ICommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        TEntity Upsert(TEntity entity, bool includeDeleted = false);

        void Delete(Identifier id, bool destroy = true);

        TEntity ResurrectDeleted(Identifier id);

        TEntity Get(Identifier id, bool includeDeleted = false);

        long Count();

        void DestroyAll();
    }
}