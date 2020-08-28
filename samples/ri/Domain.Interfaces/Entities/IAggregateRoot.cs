using System;

namespace Domain.Interfaces.Entities
{
    public interface IAggregateRootEntity : IPersistableAggregateRoot, IPublishingEntity, IPublishedEntityEventHandler
    {
        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}