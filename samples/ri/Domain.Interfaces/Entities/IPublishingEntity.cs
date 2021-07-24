namespace Domain.Interfaces.Entities
{
    public interface IPublishingEntity
    {
        void RaiseEvent(IChangeEvent @event, bool validate);
    }
}