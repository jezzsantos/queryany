using System.Collections.Generic;
using System.Linq;
using ApplicationServices;
using Domain.Interfaces.Entities;
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
            IChangeEventMigrator migrator,
            IEnumerable<IDomainEventPublisherSubscriberPair> pubSubPairs,
            params IEventNotifyingStorage[] eventingStorages) : base(logger, eventingStorages)
        {
            logger.GuardAgainstNull(nameof(logger));
            migrator.GuardAgainstNull(nameof(migrator));
            pubSubPairs.GuardAgainstNull(nameof(pubSubPairs));
            this.notificationProducer = new DomainEventNotificationProducer(logger, migrator, pubSubPairs.ToArray());
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.notificationProducer.WriteEventStream(streamName, eventStream);
        }
    }
}