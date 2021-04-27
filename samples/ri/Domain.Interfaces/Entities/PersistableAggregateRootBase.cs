using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Domain.Interfaces.Entities
{
    public abstract class PersistableAggregateRootBase : PersistableEntityBase
    {
        protected PersistableAggregateRootBase(ILogger logger, IIdentifierFactory idFactory)
            : base(logger, idFactory)
        {
        }

        protected PersistableAggregateRootBase(Identifier identifier, IDependencyContainer container,
            IReadOnlyDictionary<string, object> properties)
            : base(identifier, container, properties)
        {
        }
    }
}