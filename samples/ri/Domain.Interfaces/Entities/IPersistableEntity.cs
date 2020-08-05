using System.Collections.Generic;
using QueryAny;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableEntity : IModifiableEntity, IIdentifiableEntity, IQueryableEntity
    {
        Dictionary<string, object> Dehydrate();

        void Rehydrate(IReadOnlyDictionary<string, object> properties);
    }
}