using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.ReadModels
{
    public class ReadModelSubscription<TAggregateRoot> : IReadModelSubscription<TAggregateRoot>, IDisposable
        where TAggregateRoot : IPersistableAggregateRoot
    {
        private readonly ILogger logger;
        private readonly IReadModelStorage readModelStorage;
        private readonly IEventingStorage<TAggregateRoot> storage;

        public ReadModelSubscription(ILogger logger, IEventingStorage<TAggregateRoot> storage,
            IReadModelStorage readModelStorage)
        {
            logger.GuardAgainstNull(nameof(logger));
            storage.GuardAgainstNull(nameof(storage));
            readModelStorage.GuardAgainstNull(nameof(readModelStorage));

            this.logger = logger;
            this.storage = storage;
            this.readModelStorage = readModelStorage;

            this.storage.OnEventStreamStateChanged += OnEventStreamStateChanged;
        }

        public void Dispose()
        {
            this.storage.OnEventStreamStateChanged -= OnEventStreamStateChanged;
        }

        private void OnEventStreamStateChanged(object sender, EventStreamStateChangedArgs args)
        {
            var streamName = args.StreamName;
            var startingAt = args.StartingAt;
            try
            {
                var checkpoint = this.readModelStorage.ReadCheckpoint(streamName);
                EnsureVersion(streamName, checkpoint, startingAt);
                ReadEvents(streamName, checkpoint, args.Events);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to update ReadModel for: '{streamName}'");
                throw;
            }
        }

        private static void EnsureVersion(string streamName, long checkpoint, long startingAt)
        {
            if (startingAt > checkpoint + 1)
            {
                //TODO: load whole stream first
                throw new InvalidOperationException(
                    $"The event stream {streamName} is at checkpoint '{checkpoint}', but new events are at version {startingAt}. Perhaps some event history is missing?");
            }
        }

        private void ReadEvents(string streamName, long checkpoint, List<EventStreamStateChangeEvent> events)
        {
            events.ForEach(@event =>
            {
                this.readModelStorage.WriteEvent(@event);
                checkpoint++;
            });

            this.readModelStorage.WriteCheckPoint(streamName, checkpoint);
        }
    }
}