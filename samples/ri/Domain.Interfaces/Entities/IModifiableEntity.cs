using System;

namespace Services.Interfaces.Entities
{
    public interface IModifiableEntity
    {
        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}