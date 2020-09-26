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
    public sealed class ReadModelProjector : IReadModelProjector
    {
        private readonly IReadModelCheckpointStore checkpointStore;
        private readonly ILogger logger;
        private readonly IReadOnlyList<IReadModelProjection> projections;

        public ReadModelProjector(ILogger logger, IReadModelCheckpointStore checkpointStore,
            params IReadModelProjection[] projections)
        {
            logger.GuardAgainstNull(nameof(logger));
            checkpointStore.GuardAgainstNull(nameof(checkpointStore));
            projections.GuardAgainstNull(nameof(projections));

            this.logger = logger;
            this.checkpointStore = checkpointStore;
            this.projections = projections;
        }

        public void WriteEventStream(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            streamName.GuardAgainstNullOrEmpty(nameof(streamName));
            eventStream.GuardAgainstNull(nameof(eventStream));

            if (!eventStream.Any())
            {
                return;
            }

            try
            {
                var streamEntityType = eventStream.First().EntityType;
                var firstEventVersion = eventStream.First().Version;
                var projection = GetProjectionForStream(this.projections, streamEntityType);

                var checkpoint = this.checkpointStore.LoadCheckpoint(streamName);

                EnsureNextVersion(streamName, checkpoint, firstEventVersion);

                var processed = 0;
                foreach (var changeEvent in SkipPreviouslyProjectedVersions(eventStream, checkpoint))
                {
                    var @event = DeserializeEvent(changeEvent);

                    ProjectEvent(projection, @event, changeEvent);

                    processed++;
                }
                var newCheckpoint = checkpoint + processed;
                this.checkpointStore.SaveCheckpoint(streamName, newCheckpoint);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex,
                    $"Failed to project events in event stream '{streamName}'");
                throw;
            }
        }

        private static void ProjectEvent(IReadModelProjection projection, IChangeEvent @event,
            EventStreamStateChangeEvent changeEvent)
        {
            if (!projection.Project(@event))
            {
                throw new InvalidOperationException(
                    $"The projection '{projection.GetType().Name}' did not handle the event '{changeEvent.Id}' with event type '{changeEvent.Metadata.Fqn}'. Aborting projections");
            }
        }

        private static IEnumerable<EventStreamStateChangeEvent> SkipPreviouslyProjectedVersions(
            IEnumerable<EventStreamStateChangeEvent> eventStream, long checkpoint)
        {
            return eventStream
                .Where(e => e.Version >= checkpoint);
        }

        private static IReadModelProjection GetProjectionForStream(IEnumerable<IReadModelProjection> projections,
            string entityTypeName)
        {
            var projection = projections.FirstOrDefault(prj => prj.EntityType.Name == entityTypeName);
            if (projection == null)
            {
                throw new InvalidOperationException(
                    $"No projection is configured for entity type '{entityTypeName}'. Aborting");
            }

            return projection;
        }

        private static IChangeEvent DeserializeEvent(EventStreamStateChangeEvent changeEvent)
        {
            return changeEvent.Metadata.CreateEventFromJson(changeEvent.Id, changeEvent.Data);
        }

        private static void EnsureNextVersion(string streamName, long checkpoint, long firstEventVersion)
        {
            if (firstEventVersion > checkpoint)
            {
                throw new InvalidOperationException(
                    $"The event stream {streamName} is at checkpoint '{checkpoint}', but new events are at version {firstEventVersion}. Perhaps some event history is missing?");
            }
        }
    }
}