using System;
using ApplicationServices;
using Domain.Interfaces.Entities;
using PersonsDomain;

namespace InfrastructureServices.Eventing.Notifications
{
    public class PersonDomainEventPublisher : IDomainEventPublisher
    {
        public Type EntityType => typeof(PersonEntity);

        public IChangeEvent Publish(IChangeEvent originalEvent)
        {
            return originalEvent;
        }
    }
}