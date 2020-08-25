using System;

namespace Domain.Interfaces.Entities
{
    public interface IEntity : IPersistableEntity, IPublishingEntity, IPublishedEntityEventHandler
    {
        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}