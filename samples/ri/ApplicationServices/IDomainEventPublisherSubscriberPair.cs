namespace ApplicationServices
{
    public interface IDomainEventPublisherSubscriberPair
    {
        IDomainEventPublisher Publisher { get; }

        IDomainEventSubscriber Subscriber { get; }
    }
}