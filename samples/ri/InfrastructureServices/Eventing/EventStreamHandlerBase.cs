using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using InfrastructureServices.Properties;
using Storage.Interfaces;

namespace InfrastructureServices.Eventing
{
    public abstract class EventStreamHandlerBase : IDisposable
    {
        private readonly IEventNotifyingStorage[] eventingStorages;
        private readonly IRecorder recorder;
        private bool isStarted;

        protected EventStreamHandlerBase(IRecorder recorder, params IEventNotifyingStorage[] eventingStorages)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            eventingStorages.GuardAgainstNull(nameof(eventingStorages));

            this.recorder = recorder;
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
                    this.recorder.TraceDebug("Subscribed to events for {Storage}", storage.GetType().Name);
                }
                this.isStarted = true;
            }
        }

        protected internal void OnEventStreamStateChanged(object sender, EventStreamStateChangedArgs args)
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
                        HandleStreamEvents(streamName, eventStream);
                    }
                    catch (Exception ex)
                    {
                        ProcessingErrors.Add(
                            new EventProcessingError(ex, streamName));

                        //Continue onto next stream
                    }
                }
            });
        }

        protected abstract void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream);

        private static void EnsureContiguousVersions(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            if (!eventStream.HasContiguousVersions())
            {
                throw new InvalidOperationException(
                    Resources.EventStreamHandlerBase_OutOfOrderEvents.Format(streamName));
            }
        }

        private void WithProcessMonitoring(Action process)
        {
            ProcessingErrors.Clear();

            process.Invoke();

            if (ProcessingErrors.Any())
            {
                ProcessingErrors.ForEach(error =>
                    this.recorder.TraceError(error.Exception,
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