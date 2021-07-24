namespace Domain.Interfaces.Entities
{
    public interface IPublishedEntityEventHandler : IIdentifiableEntity
    {
        void HandleEvent(IChangeEvent @event);
    }
}