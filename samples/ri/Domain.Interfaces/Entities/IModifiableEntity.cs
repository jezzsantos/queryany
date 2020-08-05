using System;

namespace Domain.Interfaces.Entities
{
    public interface IModifiableEntity
    {
        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}