using System.Collections.Generic;
using Common;

namespace Domain.Interfaces.Entities
{
    public abstract class PersistableAggregateRootBase : PersistableEntityBase
    {
        protected PersistableAggregateRootBase(IRecorder recorder, IIdentifierFactory idFactory)
            : base(recorder, idFactory)
        {
        }

        protected PersistableAggregateRootBase(Identifier identifier, IDependencyContainer container,
            IReadOnlyDictionary<string, object> properties)
            : base(identifier, container, properties)
        {
        }
    }
}