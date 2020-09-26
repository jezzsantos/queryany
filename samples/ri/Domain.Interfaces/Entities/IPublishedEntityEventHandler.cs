namespace Domain.Interfaces.Entities
{
    public interface IPublishedEntityEventHandler
    {
        void HandleEvent(IChangeEvent @event);
    }
}