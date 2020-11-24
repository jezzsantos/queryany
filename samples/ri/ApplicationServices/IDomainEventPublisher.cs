using System;
using Domain.Interfaces.Entities;

namespace ApplicationServices
{
    public interface IDomainEventPublisher
    {
        Type EntityType { get; }

        IChangeEvent Publish(IChangeEvent originalEvent);
    }
}