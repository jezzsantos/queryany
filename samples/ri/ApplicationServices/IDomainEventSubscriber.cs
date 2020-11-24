using Domain.Interfaces.Entities;

namespace ApplicationServices
{
    public interface IDomainEventSubscriber
    {
        bool Notify(IChangeEvent originalEvent);
    }
}