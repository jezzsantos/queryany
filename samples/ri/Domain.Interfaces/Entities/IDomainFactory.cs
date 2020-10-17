using System;
using System.Collections.Generic;
using System.Reflection;

namespace Domain.Interfaces.Entities
{
    public interface IDomainFactory
    {
        IPersistableAggregateRoot RehydrateAggregateRoot(Type entityType,
            IReadOnlyDictionary<string, object> rehydratingPropertyValues);
        
        IPersistableEntity RehydrateEntity(Type entityType,
            IReadOnlyDictionary<string, object> rehydratingPropertyValues);

        IPersistableValueObject RehydrateValueObject(Type valueObjectType, string rehydratingPropertyValue);

        void RegisterDomainTypesFromAssemblies(params Assembly[] assembliesContainingFactories);
    }
}