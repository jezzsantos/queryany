using System;
using QueryAny;

namespace Application.Storage.Interfaces
{
    public interface IPersistableDto : IHasIdentity, IQueryableEntity
    {
        DateTime? LastPersistedAtUtc { get; }

        bool? IsDeleted { get; }
    }
}