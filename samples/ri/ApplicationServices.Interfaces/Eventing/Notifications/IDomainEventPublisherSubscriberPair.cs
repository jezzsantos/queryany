namespace ApplicationServices.Interfaces.Eventing.Notifications
{
    public interface IDomainEventPublisherSubscriberPair
    {
        IDomainEventPublisher Publisher { get; }

        IDomainEventSubscriber Subscriber { get; }
    }
}