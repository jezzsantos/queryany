using System;

namespace Storage.Interfaces
{
    public interface IModifiableEntity
    {
        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}