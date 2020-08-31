using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface ICommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        IDomainFactory DomainFactory { get; }

        TEntity Upsert(TEntity entity);

        void Delete(Identifier id);

        TEntity Get(Identifier id);

        long Count();

        void DestroyAll();
    }
}