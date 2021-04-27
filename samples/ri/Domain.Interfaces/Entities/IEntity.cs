﻿using System;
using QueryAny;

namespace Domain.Interfaces.Entities
{
    public interface IEntity : IIdentifiableEntity, IQueryableEntity, IPublishingEntity, IPublishedEntityEventHandler
    {
        DateTime? LastPersistedAtUtc { get; }

        bool? IsDeleted { get; }

        DateTime CreatedAtUtc { get; }

        DateTime LastModifiedAtUtc { get; }
    }
}