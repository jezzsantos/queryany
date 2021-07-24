using System;
using Domain.Interfaces.Entities;

namespace ApplicationServices.Interfaces.Eventing.Notifications
{
    public interface IDomainEventPublisher
    {
        Type EntityType { get; }

        IChangeEvent Publish(IChangeEvent originalEvent);
    }
}