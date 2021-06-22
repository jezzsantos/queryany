using System;
using System.Collections.Generic;
using System.Linq;
using Application.Storage.Interfaces;
using Application.Storage.Interfaces.ReadModels;
using Common;
using Domain.Interfaces.Entities;
using ServiceStack;
using Storage.ReadModels.Properties;

namespace Storage.ReadModels
{
    public sealed class ReadModelProjector : IReadModelProjector
    {
        private readonly IReadModelCheckpointStore checkpointStore;
        private readonly IChangeEventMigrator migrator;
        private readonly IReadOnlyList<IReadModelProjection> projections;
        private readonly IRecorder recorder;

        public ReadModelProjector(IRecorder recorder, IReadModelCheckpointStore checkpointStore,
            IChangeEventMigrator migrator,
            params IReadModelProjection[] projections)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            checkpointStore.GuardAgainstNull(nameof(checkpointStore));
            projections.GuardAgainstNull(nameof(projections));
            migrator.GuardAgainstNull(nameof(migrator));

            this.recorder = recorder;
            this.checkpointStore = checkpointStore;
            this.projections = projections;
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
                    var @event = DeserializeEvent(changeEvent, this.migrator);

                    ProjectEvent(projection, @event, changeEvent);

                    processed++;
                }
                var newCheckpoint = checkpoint + processed;
                this.checkpointStore.SaveCheckpoint(streamName, newCheckpoint);
            }
            catch (Exception ex)
            {
                this.recorder.TraceError(ex,
                    $"Failed to project events in event stream '{streamName}'");
                throw new InvalidOperationException(Resources.ReadModelProjector_UnexpectedError, ex);
            }
        }

        private static void ProjectEvent(IReadModelProjection projection, IChangeEvent @event,
            EventStreamStateChangeEvent changeEvent)
        {
            if (!projection.Project(@event))
            {
                throw new InvalidOperationException(
                    Resources.ReadModelProjector_ProjectionError.Fmt(projection.GetType().Name, changeEvent.Id,
                        changeEvent.Metadata.Fqn));
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
                    Resources.ReadModelProjector_ProjectionNotConfigured.Fmt(entityTypeName));
            }

            return projection;
        }

        private static IChangeEvent DeserializeEvent(EventStreamStateChangeEvent changeEvent,
            IChangeEventMigrator migrator)
        {
            return changeEvent.Metadata.CreateEventFromJson(changeEvent.Id, changeEvent.Data, migrator);
        }

        private static void EnsureNextVersion(string streamName, long checkpoint, long firstEventVersion)
        {
            if (firstEventVersion > checkpoint)
            {
                throw new InvalidOperationException(
                    Resources.ReadModelProjector_CheckpointError.Fmt(streamName, checkpoint, firstEventVersion));
            }
        }
    }
}