using System;
using QueryAny;

namespace Storage.Interfaces
{
    public interface IPersistableDto : IHasIdentity, IQueryableEntity
    {
        DateTime? LastPersistedAtUtc { get; }

        bool? IsDeleted { get; }
    }
}