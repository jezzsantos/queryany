using System;

namespace Domain.Interfaces.Entities
{
    public interface IAggregateRootEntity : IPersistableEntity, IPublishingAggregateEntity, IPublishedEntityEventHandler
    {
        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}