using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public delegate TEntity EntityFactory<out TEntity>(IReadOnlyDictionary<string, object> hydratingProperties)
        where TEntity : IPersistableEntity;
}