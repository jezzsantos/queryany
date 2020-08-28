namespace Domain.Interfaces.Entities
{
    public delegate TEntity EntityFactory<out TEntity>(Identifier identifier,
        IDependencyContainer container)
        where TEntity : IPersistableEntity;

    public delegate TValueObject ValueObjectFactory<out TValueObject>(string hydratingProperty,
        IDependencyContainer container)
        where TValueObject : IPersistableValueObject;
}