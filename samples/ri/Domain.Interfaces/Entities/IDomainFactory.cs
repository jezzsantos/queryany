using System;
using System.Collections.Generic;
using System.Reflection;

namespace Domain.Interfaces.Entities
{
    public interface IDomainFactory
    {
        IPersistableEntity RehydrateEntity(Type entityType,
            IReadOnlyDictionary<string, object> rehydratingPropertyValues);

        IPersistableValueObject RehydrateValueObject(Type valueObjectType, string rehydratingPropertyValue);

        void RegisterTypesFromAssemblies(params Assembly[] assembliesContainingFactories);
    }
}