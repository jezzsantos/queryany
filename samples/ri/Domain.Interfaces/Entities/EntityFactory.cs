using System.Collections.Generic;
using Common;

namespace Domain.Interfaces.Entities
{
    public delegate TEntity AggregateRootFactory<out TEntity>(Identifier identifier,
        IDependencyContainer container, IReadOnlyDictionary<string, object> rehydratingProperties)
        where TEntity : IPersistableAggregateRoot;

    public delegate TEntity EntityFactory<out TEntity>(Identifier identifier,
        IDependencyContainer container, IReadOnlyDictionary<string, object> rehydratingProperties)
        where TEntity : IPersistableEntity;

    public delegate TValueObject ValueObjectFactory<out TValueObject>(string hydratingProperty,
        IDependencyContainer container)
        where TValueObject : IPersistableValueObject;
}