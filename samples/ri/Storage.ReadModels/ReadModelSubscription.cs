using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;

namespace Storage.ReadModels
{
    public class ReadModelSubscription<TAggregateRoot> : IReadModelSubscription, IDisposable
        where TAggregateRoot : IPersistableAggregateRoot
    {
        private readonly IEventingStorage<TAggregateRoot> eventingStorage;
        private readonly ILogger logger;
        private readonly IReadModelProjector projector;
        private bool isStarted;

        public ReadModelSubscription(ILogger logger, IEventingStorage<TAggregateRoot> eventingStorage,
            IReadModelProjector readModelProjector)
        {
            logger.GuardAgainstNull(nameof(logger));
            eventingStorage.GuardAgainstNull(nameof(eventingStorage));
            readModelProjector.GuardAgainstNull(nameof(readModelProjector));

            this.logger = logger;
            this.eventingStorage = eventingStorage;
            this.projector = readModelProjector;
            ProcessingErrors = new List<EventProcessingError>();
        }

        internal List<EventProcessingError> ProcessingErrors { get; }

        public void Dispose()
        {
            if (this.isStarted)
            {
                this.eventingStorage.OnEventStreamStateChanged -= OnEventStreamStateChanged;
            }
        }

        public void Start()
        {
            if (!this.isStarted)
            {
                this.eventingStorage.OnEventStreamStateChanged += OnEventStreamStateChanged;
                this.isStarted = true;
                this.logger.LogDebug("Subscribed to read model changes for {Aggregate}", typeof(TAggregateRoot).Name);
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

    internal static class EventStreamExtensions
    {
        public static bool HasContiguousVersions(this List<EventStreamStateChangeEvent> events)
        {
            if (!events.Any())
            {
                return true;
            }

            static IEnumerable<long> GetRange(long start, long count)
            {
                for (long next = 0; next < count; next++)
                {
                    yield return start + next;
                }
            }

            var expectedRange = GetRange(events.First().Version, events.Count);
            return events.Select(e => e.Version).SequenceEqual(expectedRange);
        }
    }
}