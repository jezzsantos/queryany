using System.Collections.Generic;
using QueryAny;

namespace Storage.Interfaces
{
    public interface IPersistableEntity : IIdentifyableEntity, INamedEntity
    {
        Dictionary<string, object> Dehydrate();

        void Rehydrate(IReadOnlyDictionary<string, object> properties);
    }
}