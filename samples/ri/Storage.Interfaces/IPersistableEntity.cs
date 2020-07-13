using System.Collections.Generic;
using QueryAny;
using Services.Interfaces.Entities;

namespace Storage.Interfaces
{
    public interface IPersistableEntity : IModifiableEntity, IIdentifiableEntity, INamedEntity
    {
        Dictionary<string, object> Dehydrate();

        void Rehydrate(IReadOnlyDictionary<string, object> properties);
    }
}