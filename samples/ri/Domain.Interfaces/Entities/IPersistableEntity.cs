using System;
using System.Collections.Generic;
using QueryAny;

namespace Domain.Interfaces.Entities
{
    public interface IPersistableEntity : IIdentifiableEntity, IQueryableEntity
    {
        DateTime? LastPersistedAtUtc { get; }

        bool? IsDeleted { get; }

        Dictionary<string, object> Dehydrate();
    }
}