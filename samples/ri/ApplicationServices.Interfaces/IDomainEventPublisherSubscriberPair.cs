namespace ApplicationServices.Interfaces
{
    public interface IDomainEventPublisherSubscriberPair
    {
        IDomainEventPublisher Publisher { get; }

        IDomainEventSubscriber Subscriber { get; }
    }
}