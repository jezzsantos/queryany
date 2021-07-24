using Domain.Interfaces.Entities;

namespace ApplicationServices.Interfaces.Eventing.Notifications
{
    public interface IDomainEventSubscriber
    {
        bool Notify(IChangeEvent originalEvent);
    }
}