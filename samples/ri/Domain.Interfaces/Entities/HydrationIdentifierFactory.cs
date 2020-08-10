using System;
using System.Collections.Generic;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    public class HydrationIdentifierFactory : IIdentifierFactory
    {
        private readonly IReadOnlyDictionary<string, object> hydratingProperties;

        public HydrationIdentifierFactory(IReadOnlyDictionary<string, object> hydratingProperties)
        {
            hydratingProperties.GuardAgainstNull(nameof(hydratingProperties));
            this.hydratingProperties = hydratingProperties;
        }

        public Identifier Create(IIdentifiableEntity entity)
        {
            var id = this.hydratingProperties.GetValueOrDefault<Identifier>(nameof(IIdentifiableEntity.Id));
            if (!id.HasValue())
            {
                throw new ResourceConflictException("The hydrating properties does not contain an Identifier");
            }

            return id;
        }

        public bool IsValid(Identifier value)
        {
            throw new NotImplementedException();
        }
    }
}