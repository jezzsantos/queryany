using System;
using System.Collections.Generic;
using Application.Storage.Interfaces;
using Application.Storage.Interfaces.ReadModels;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using Storage.ReadModels;
using Storage.ReadModels.Properties;
using UnitTesting.Common;
using Xunit;

namespace Storage.UnitTests.ReadModels
{
    [Trait("Category", "Unit")]
    public class ReadModelProjectorSpec
    {
        private readonly Mock<IReadModelCheckpointStore> checkpointStore;
        private readonly Mock<IReadModelProjection> projection;
        private readonly ReadModelProjector projector;

        public ReadModelProjectorSpec()
        {
            var recorder = new Mock<IRecorder>();
            this.checkpointStore = new Mock<IReadModelCheckpointStore>();
            var changeEventTypeMigrator = new ChangeEventTypeMigrator();
            this.projection = new Mock<IReadModelProjection>();
            this.projection.Setup(prj => prj.EntityType)
                .Returns(typeof(string));
            this.projection.Setup(prj => prj.Project(It.IsAny<IChangeEvent>()))
                .Returns(true);
            var projections = new List<IReadModelProjection> {this.projection.Object};
            this.projector = new ReadModelProjector(recorder.Object, this.checkpointStore.Object,
                changeEventTypeMigrator, projections.ToArray());
        }

        [Fact]
        public void WhenWriteEventStreamAndNoEvents_ThenReturns()
        {
            this.projector.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>());

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()), Times.Never);
            this.projection.Verify(prj => prj.Project(It.IsAny<IChangeEvent>()), Times.Never);
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint(It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public void WhenWriteEventStreamAndNoConfiguredProjection_ThenThrows()
        {
            this.projection.Setup(prj => prj.EntityType)
                .Returns(typeof(string));
            this.projector
                .Invoking(x => x.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        EntityType = "atypename"
                    }
                })).Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ReadModelProjector_UnexpectedError);

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()), Times.Never);
            this.projection.Verify(prj => prj.Project(It.IsAny<IChangeEvent>()), Times.Never);
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint(It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public void WhenWriteEventStreamAndEventVersionGreaterThanCheckpoint_ThenThrows()
        {
            this.checkpointStore.Setup(cs => cs.LoadCheckpoint("astreamname"))
                .Returns(5);

            this.projection.Setup(prj => prj.EntityType)
                .Returns(typeof(string));
            this.projector
                .Invoking(x => x.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        EntityType = nameof(String),
                        Version = 6
                    }
                })).Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ReadModelProjector_UnexpectedError);
        }

        [Fact]
        public void WhenWriteEventStreamAndEventVersionLessThanCheckpoint_ThenSkipsPreviousVersions()
        {
            this.checkpointStore.Setup(cs => cs.LoadCheckpoint("astreamname"))
                .Returns(5);

            this.projector.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
            {
                new EventStreamStateChangeEvent
                {
                    Id = "anid1",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid1"}),
                    Version = 4,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid2"}),
                    Version = 5,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid3"}),
                    Version = 6,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                }
            });

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid1"
            )), Times.Never);
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid3"
            )));
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint("astreamname", 7));
        }

        [Fact]
        public void WhenWriteEventStreamAndDeserializationOfEventsFails_ThenThrows()
        {
            this.checkpointStore.Setup(cs => cs.LoadCheckpoint("astreamname"))
                .Returns(3);

            this.projector
                .Invoking(x => x.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        Id = "anid",
                        EntityType = nameof(String),
                        Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid"}),
                        Version = 3,
                        Metadata = new EventMetadata("unknowntype")
                    }
                })).Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ReadModelProjector_UnexpectedError);
        }

        [Fact]
        public void WhenWriteEventStreamAndFirstEverEvent_ThenProjectsEvents()
        {
            const long startingCheckpoint = ReadModelCheckpointStore.StartingCheckpointPosition;
            this.checkpointStore.Setup(cs => cs.LoadCheckpoint("astreamname"))
                .Returns(startingCheckpoint);

            this.projector.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
            {
                new EventStreamStateChangeEvent
                {
                    Id = "anid1",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid1"}),
                    Version = startingCheckpoint,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid2"}),
                    Version = startingCheckpoint + 1,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid3"}),
                    Version = startingCheckpoint + 2,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                }
            });

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid1"
            )));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid3"
            )));
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint("astreamname", startingCheckpoint + 3));
        }

        [Fact]
        public void WhenWriteEventStream_ThenProjectsEvents()
        {
            this.checkpointStore.Setup(cs => cs.LoadCheckpoint("astreamname"))
                .Returns(3);

            this.projector.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
            {
                new EventStreamStateChangeEvent
                {
                    Id = "anid1",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid1"}),
                    Version = 3,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid2"}),
                    Version = 4,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestEvent {EntityId = "aneventid3"}),
                    Version = 5,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                }
            });

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid1"
            )));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.projection.Verify(prj => prj.Project(It.Is<TestEvent>(e =>
                e.EntityId == "aneventid3"
            )));
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint("astreamname", 6));
        }
    }

    public class TestEvent : IChangeEvent
    {
        public string EntityId { get; set; }

        public DateTime ModifiedUtc { get; set; }
    }
}