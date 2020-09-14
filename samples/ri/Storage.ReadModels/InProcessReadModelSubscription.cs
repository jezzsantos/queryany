using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;

namespace Storage.ReadModels
{
    /// <summary>
    ///     Defines a subscription that connects directly to one or more <see cref="IEventPublishingStorage" /> instances, to
    ///     receive events.
    /// </summary>
    public class InProcessReadModelSubscription : IReadModelSubscription, IDisposable
    {
        private readonly IEventPublishingStorage[] eventingStorages;
        private readonly ILogger logger;
        private readonly IReadModelProjector projector;
        private bool isStarted;

        public InProcessReadModelSubscription(ILogger logger, IReadModelProjector readModelProjector,
            params IEventPublishingStorage[] eventingStorages)
        {
            logger.GuardAgainstNull(nameof(logger));
            readModelProjector.GuardAgainstNull(nameof(readModelProjector));
            eventingStorages.GuardAgainstNull(nameof(eventingStorages));

            this.logger = logger;
            this.projector = readModelProjector;
            this.eventingStorages = eventingStorages;
            ProcessingErrors = new List<EventProcessingError>();
        }

        internal List<EventProcessingError> ProcessingErrors { get; }

        public void Dispose()
        {
            if (this.isStarted)
            {
                foreach (var storage in this.eventingStorages)
                {
                    storage.OnEventStreamStateChanged -= OnEventStreamStateChanged;
                }
            }
        }

        public void Start()
        {
            if (!this.isStarted)
            {
                foreach (var storage in this.eventingStorages)
                {
                    storage.OnEventStreamStateChanged += OnEventStreamStateChanged;
                    this.logger.LogDebug("Subscribed to events for {Storage}", storage.GetType().Name);
                }
                this.isStarted = true;
            }
        }

        internal void OnEventStreamStateChanged(object sender, EventStreamStateChangedArgs args)
        {
            var allEvents = args.Events;
            if (!allEvents.Any())
            {
                return;
            }

            WithProcessMonitoring(() =>
            {
                var eventsStreams = allEvents.GroupBy(e => e.StreamName)
                    .Select(grp => grp.AsEnumerable())
                    .Select(grp => grp.OrderBy(e => e.Version).ToList());

                foreach (var eventStream in eventsStreams)
                {
                    var firstEvent = eventStream.First();
                    var streamName = firstEvent.StreamName;

                    try
                    {
                        EnsureContiguousVersions(streamName, eventStream);
                        this.projector.WriteEventStream(streamName, eventStream);
                    }
                    catch (Exception ex)
                    {
                        ProcessingErrors.Add(new EventProcessingError(ex, streamName));

                        //Continue onto next stream
                    }
                }
            });
        }

        private static void EnsureContiguousVersions(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            if (!eventStream.HasContiguousVersions())
            {
                throw new InvalidOperationException(
                    $"The event stream {streamName} contains events with out of order versions.");
            }
        }

        private void WithProcessMonitoring(Action process)
        {
            ProcessingErrors.Clear();

            process.Invoke();

            if (ProcessingErrors.Any())
            {
                ProcessingErrors.ForEach(error =>
                    this.logger.LogError(error.Exception,
                        "Failed to relay new events to read model for: {StreamName}", error.StreamName));
            }
        }

        internal class EventProcessingError
        {
            public EventProcessingError(Exception ex, string streamName)
            {
                Exception = ex;
                StreamName = streamName;
            }

            public string StreamName { get; }

            public Exception Exception { get; }
        }
    }
}