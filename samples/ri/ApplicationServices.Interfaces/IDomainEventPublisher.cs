using System;
using Domain.Interfaces.Entities;

namespace ApplicationServices.Interfaces
{
    public interface IDomainEventPublisher
    {
        Type EntityType { get; }

        IChangeEvent Publish(IChangeEvent originalEvent);
    }
}