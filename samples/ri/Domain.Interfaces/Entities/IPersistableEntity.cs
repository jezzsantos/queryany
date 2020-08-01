using System.Collections.Generic;
using QueryAny;

namespace Services.Interfaces.Entities
{
    public interface IPersistableEntity : IModifiableEntity, IIdentifiableEntity, IQueryableEntity
    {
        Dictionary<string, object> Dehydrate();

        void Rehydrate(IReadOnlyDictionary<string, object> properties);
    }
}