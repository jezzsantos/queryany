using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceStack;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;
using Storage.ReadModels;

namespace Storage.UnitTests.ReadModels
{
    [TestClass, TestCategory("Unit")]
    public class ReadModelProjectorSpec
    {
        private Mock<IReadModelCheckpointStore> checkpointStore;
        private Mock<ILogger> logger;
        private Mock<IReadModelProjection> projection;
        private List<IReadModelProjection> projections;
        private ReadModelProjector projector;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.checkpointStore = new Mock<IReadModelCheckpointStore>();
            this.projection = new Mock<IReadModelProjection>();
            this.projection.Setup(prj => prj.EntityType)
                .Returns(typeof(string));
            this.projection.Setup(prj => prj.Project(It.IsAny<IChangeEvent>()))
                .Returns(true);
            this.projections = new List<IReadModelProjection> {this.projection.Object};
            this.projector = new ReadModelProjector(this.logger.Object, this.checkpointStore.Object,
                this.projections.ToArray());
        }

        [TestMethod]
        public void WhenWriteEventStreamAndNoEvents_ThenReturns()
        {
            this.projector.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>());

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()), Times.Never);
            this.projection.Verify(prj => prj.Project(It.IsAny<IChangeEvent>()), Times.Never);
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint(It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        }

        [TestMethod]
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
                })).Should().Throw<InvalidOperationException>();

            this.checkpointStore.Verify(cs => cs.LoadCheckpoint(It.IsAny<string>()), Times.Never);
            this.projection.Verify(prj => prj.Project(It.IsAny<IChangeEvent>()), Times.Never);
            this.checkpointStore.Verify(cs => cs.SaveCheckpoint(It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        }

        [TestMethod]
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
                })).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
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
                    Data = new TestEvent {EntityId = "aneventid1"}.ToJson(),
                    Version = 4,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = new TestEvent {EntityId = "aneventid2"}.ToJson(),
                    Version = 5,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = new TestEvent {EntityId = "aneventid3"}.ToJson(),
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

        [TestMethod]
        public void WhenWriteEventStreamAndDeserializationOfEventsFails_ThenThrows()
        {
            this.checkpointStore.Setup(cs => cs.LoadCheckpoint("astreamname"))
                .Returns(3);

            this.projector
                .Invoking(x => x.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        EntityType = nameof(String),
                        Data = new TestEvent {EntityId = "aneventid"}.ToJson(),
                        Version = 4,
                        Metadata = new EventMetadata("unknowntype")
                    }
                })).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
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
                    Data = new TestEvent {EntityId = "aneventid1"}.ToJson(),
                    Version = startingCheckpoint,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = new TestEvent {EntityId = "aneventid2"}.ToJson(),
                    Version = startingCheckpoint + 1,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = new TestEvent {EntityId = "aneventid3"}.ToJson(),
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

        [TestMethod]
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
                    Data = new TestEvent {EntityId = "aneventid1"}.ToJson(),
                    Version = 3,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = new TestEvent {EntityId = "aneventid2"}.ToJson(),
                    Version = 4,
                    Metadata = new EventMetadata(typeof(TestEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = new TestEvent {EntityId = "aneventid3"}.ToJson(),
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