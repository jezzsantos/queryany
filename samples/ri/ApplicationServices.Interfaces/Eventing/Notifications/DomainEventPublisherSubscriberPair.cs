namespace ApplicationServices.Interfaces.Eventing.Notifications
{
    public class DomainEventPublisherSubscriberPair : IDomainEventPublisherSubscriberPair
    {
        public static readonly IDomainEventPublisherSubscriberPair[] None = new IDomainEventPublisherSubscriberPair[0];

        public DomainEventPublisherSubscriberPair(IDomainEventPublisher publisher, IDomainEventSubscriber subscriber)
        {
            Publisher = publisher;
            Subscriber = subscriber;
        }

        public IDomainEventPublisher Publisher { get; }

        public IDomainEventSubscriber Subscriber { get; }
    }
}