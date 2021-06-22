using System.Collections.Generic;
using System.Linq;
using Application.Storage.Interfaces;
using ApplicationServices.Interfaces;
using Common;
using Domain.Interfaces.Entities;

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

        public InProcessChangeEventNotificationSubscription(IRecorder recorder,
            IChangeEventMigrator migrator,
            IEnumerable<IDomainEventPublisherSubscriberPair> pubSubPairs,
            params IEventNotifyingStorage[] eventingStorages) : base(recorder, eventingStorages)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            migrator.GuardAgainstNull(nameof(migrator));
            pubSubPairs.GuardAgainstNull(nameof(pubSubPairs));
            this.notificationProducer = new DomainEventNotificationProducer(recorder, migrator, pubSubPairs.ToArray());
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.notificationProducer.WriteEventStream(streamName, eventStream);
        }
    }
}