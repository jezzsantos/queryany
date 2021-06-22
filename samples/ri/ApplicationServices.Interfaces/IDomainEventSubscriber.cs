using Domain.Interfaces.Entities;

namespace ApplicationServices.Interfaces
{
    public interface IDomainEventSubscriber
    {
        bool Notify(IChangeEvent originalEvent);
    }
}