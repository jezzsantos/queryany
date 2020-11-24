using System.Collections.Generic;
using ApplicationServices;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace InfrastructureServices.Eventing.Notifications
{
    /// <summary>
    ///     Defines a subscription that connects directly to one or more <see cref="IEventStreamStorage{TAggregateRoot}" />
    ///     instances, to listen to raised events, that are then relayed to listening applications.
    /// </summary>
    public class InProcessChangeEventNotificationSubscription : EventStreamHandlerBase,
        IChangeEventNotificationSubscription
    {
        private readonly IDomainEventNotificationProducer notificationProducer;

        public InProcessChangeEventNotificationSubscription(ILogger logger,
            IDomainEventNotificationProducer notificationProducer,
            params IEventNotifyingStorage[] eventingStorages) : base(logger, eventingStorages)
        {
            notificationProducer.GuardAgainstNull(nameof(notificationProducer));
            this.notificationProducer = notificationProducer;
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.notificationProducer.WriteEventStream(streamName, eventStream);
        }
    }
}