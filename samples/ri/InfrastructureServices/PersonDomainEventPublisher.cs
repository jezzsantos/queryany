using System;
using ApplicationServices;
using Domain.Interfaces.Entities;
using PersonsDomain;

namespace InfrastructureServices
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