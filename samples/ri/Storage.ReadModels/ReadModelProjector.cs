using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack.Text;
using Storage.Interfaces;

namespace Storage.ReadModels
{
    public class ReadModelProjector : IReadModelProjector
    {
        private readonly IReadModelCheckpointStore checkpointStore;
        private readonly ILogger logger;
        private readonly IReadOnlyList<IReadModelProjection> projections;

        public ReadModelProjector(ILogger logger, IReadModelCheckpointStore readModelCheckpointStore,
            params IReadModelProjection[] projections)
        {
            logger.GuardAgainstNull(nameof(logger));
            readModelCheckpointStore.GuardAgainstNull(nameof(readModelCheckpointStore));
            projections.GuardAgainstNull(nameof(projections));

            this.logger = logger;
            this.checkpointStore = readModelCheckpointStore;
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
                var projection = GetProjection(this.projections, streamEntityType);

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

        private static void ProjectEvent(IReadModelProjection readModelProjection, object @event,
            EventStreamStateChangeEvent changeEvent)
        {
            if (!readModelProjection.Project(@event))
            {
                throw new InvalidOperationException(
                    $"The readModelProjection '{readModelProjection.GetType().Name}' did not handle the event '{changeEvent.Id}' with event type '{changeEvent.Metadata.Fqn}'. Aborting projections");
            }
        }

        private static IEnumerable<EventStreamStateChangeEvent> SkipPreviouslyProjectedVersions(
            IEnumerable<EventStreamStateChangeEvent> eventStream, long checkpoint)
        {
            return eventStream
                .Where(e => e.Version > checkpoint);
        }

        private static IReadModelProjection GetProjection(IEnumerable<IReadModelProjection> projections,
            string entityTypeName)
        {
            var projection = projections.FirstOrDefault(prj => prj.EntityType.Name == entityTypeName);
            if (projection == null)
            {
                throw new InvalidOperationException(
                    $"No readModelProjection is configured for entity type '{entityTypeName}'. Aborting");
            }

            return projection;
        }

        private static object DeserializeEvent(EventStreamStateChangeEvent changeEvent)
        {
            try
            {
                var type = Type.GetType(changeEvent.Metadata.Fqn);
                var json = changeEvent.Data;
                return JsonSerializer.DeserializeFromString(json, type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Unable to deserialize event {changeEvent.Id}. Possibly unknown type '{changeEvent.Metadata.Fqn}'. Perhaps the type has been renamed or no longer exists?",
                    ex);
            }
        }

        private static void EnsureNextVersion(string streamName, long checkpoint, long firstEventVersion)
        {
            if (firstEventVersion > checkpoint + 1)
            {
                throw new InvalidOperationException(
                    $"The event stream {streamName} is at checkpoint '{checkpoint}', but new events are at version {firstEventVersion}. Perhaps some event history is missing?");
            }
        }
    }
}