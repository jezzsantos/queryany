namespace Domain.Interfaces.Entities
{
    public interface IPublishingEntity
    {
        void RaiseEvent(object @event);
    }
}