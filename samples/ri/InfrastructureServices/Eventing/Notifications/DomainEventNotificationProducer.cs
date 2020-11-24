using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationServices;
using Domain.Interfaces.Entities;
using InfrastructureServices.Properties;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace InfrastructureServices.Eventing.Notifications
{
    /// <summary>
    ///     A general purpose event relay for relaying events to a collection of registered
    ///     <see cref="IDomainEventPublisherSubscriberPair" /> for publishing to subscribers
    /// </summary>
    public class DomainEventNotificationProducer : IDomainEventNotificationProducer
    {
        private readonly ILogger logger;
        private readonly IChangeEventMigrator migrator;
        private readonly IReadOnlyList<IDomainEventPublisherSubscriberPair> pubSubPairs;

        public DomainEventNotificationProducer(ILogger logger, IChangeEventMigrator migrator,
            params IDomainEventPublisherSubscriberPair[] pubSubPairs)
        {
            logger.GuardAgainstNull(nameof(logger));
            pubSubPairs.GuardAgainstNull(nameof(pubSubPairs));
            migrator.GuardAgainstNull(nameof(migrator));

            this.logger = logger;
            this.pubSubPairs = pubSubPairs;
            this.migrator = migrator;
        }

        public void WriteEventStream(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            streamName.GuardAgainstNullOrEmpty(nameof(streamName));
            eventStream.GuardAgainstNull(nameof(eventStream));

            if (!eventStream.Any())
            {
                return;
            }

            if (!this.pubSubPairs.Any())
            {
                return;
            }

            try
            {
                var streamEntityType = eventStream.First().EntityType;
                var pair = GetPublisherSubscriberPairForStream(this.pubSubPairs, streamEntityType);

                foreach (var changeEvent in eventStream)
                {
                    var @event = DeserializeEvent(changeEvent, this.migrator);
                    RelayEvent(pair, @event, changeEvent);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex,
                    $"Failed to relay events in event stream '{streamName}'");
                throw new InvalidOperationException(Resources.DomainEventNotificationProducer_UnexpectedError, ex);
            }
        }

        private static void RelayEvent(IDomainEventPublisherSubscriberPair pair, IChangeEvent @event,
            EventStreamStateChangeEvent changeEvent)
        {
            var eventToPublish = pair.Publisher.Publish(@event);
            if (eventToPublish == null)
            {
                throw new InvalidOperationException(Resources.DomainEventNotificationProducer_PublisherError.Format(
                    pair.Publisher.GetType().Name,
                    changeEvent.Id, changeEvent.Metadata.Fqn));
            }

            if (!pair.Subscriber.Notify(eventToPublish))
            {
                throw new InvalidOperationException(Resources.DomainEventNotificationProducer_SubscriberError.Format(
                    pair.Subscriber.GetType().Name,
                    changeEvent.Id, changeEvent.Metadata.Fqn));
            }
        }

        private static IDomainEventPublisherSubscriberPair GetPublisherSubscriberPairForStream(
            IEnumerable<IDomainEventPublisherSubscriberPair> pubSubPairs,
            string entityTypeName)
        {
            var pair = pubSubPairs.FirstOrDefault(prj => prj.Publisher.EntityType.Name == entityTypeName);
            if (pair == null)
            {
                throw new InvalidOperationException(
                    Resources.DomainEventNotificationProducer_PublisherSubscriberPairNotConfigured.Format(
                        entityTypeName));
            }

            return pair;
        }

        private static IChangeEvent DeserializeEvent(EventStreamStateChangeEvent changeEvent,
            IChangeEventMigrator migrator)
        {
            return changeEvent.Metadata.CreateEventFromJson(changeEvent.Id, changeEvent.Data, migrator);
        }
    }
}