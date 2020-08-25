using System;
using System.Collections.Generic;
using QueryAny;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableEntity : IIdentifiableEntity, IQueryableEntity
    {
        DateTime? LastPersistedAtUtc { get; }

        Dictionary<string, object> Dehydrate();

        void Rehydrate(IReadOnlyDictionary<string, object> properties);
    }
}