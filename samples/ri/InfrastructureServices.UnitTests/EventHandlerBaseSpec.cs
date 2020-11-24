using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using InfrastructureServices.Eventing;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Storage.Interfaces;

namespace InfrastructureServices.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class EventHandlerBaseSpec
    {
        private Mock<Action<string, List<EventStreamStateChangeEvent>>> action;
        private TestEventHandler handler;

        [TestInitialize]
        public void Initialize()
        {
            this.action = new Mock<Action<string, List<EventStreamStateChangeEvent>>>();
            this.handler = new TestEventHandler(this.action);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndNoEvents_ThenDoesNotWriteEvents()
        {
            this.handler.OnEventStreamStateChanged(null,
                new EventStreamStateChangedArgs(new List<EventStreamStateChangeEvent>()));

            this.action.Verify(
                rms => rms("astreamname", It.IsAny<List<EventStreamStateChangeEvent>>()), Times.Never);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndFromDifferentStreams_ThenWritesBatchedEvents()
        {
            this.handler.OnEventStreamStateChanged(null,
                new EventStreamStateChangedArgs(new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid1",
                        StreamName = "astreamname1",
                        Version = 5
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid2",
                        StreamName = "astreamname2",
                        Version = 3
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid3",
                        StreamName = "astreamname1",
                        Version = 4
                    }
                }));

            this.action.Verify(
                rms => rms("astreamname1", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 2
                    && batch[0].Id == "aneventid3"
                    && batch[1].Id == "aneventid1"
                )), Times.Once);
            this.action.Verify(
                rms => rms("astreamname2", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 1
                    && batch[0].Id == "aneventid2"
                )), Times.Once);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndFromDifferentStreamsAndWriteFails_ThenWritesRemainingBatches()
        {
            this.action.Setup(rms =>
                    rms(It.IsAny<string>(), It.IsAny<List<EventStreamStateChangeEvent>>()))
                .Throws<Exception>();

            this.handler.OnEventStreamStateChanged(null,
                new EventStreamStateChangedArgs(new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid1",
                        StreamName = "astreamname1",
                        Version = 5
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid2",
                        StreamName = "astreamname2",
                        Version = 3
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid3",
                        StreamName = "astreamname1",
                        Version = 4
                    }
                }));

            this.action.Verify(
                rms => rms("astreamname1", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 2
                    && batch[0].Id == "aneventid3"
                    && batch[1].Id == "aneventid1"
                )), Times.Once);
            this.action.Verify(
                rms => rms("astreamname2", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 1
                    && batch[0].Id == "aneventid2"
                )), Times.Once);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndEventsAreOutOfOrder_ThenThrows()
        {
            this.handler.OnEventStreamStateChanged(null,
                new EventStreamStateChangedArgs(new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid1",
                        StreamName = "astreamname1",
                        Version = 5
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid2",
                        StreamName = "astreamname1",
                        Version = 2
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid3",
                        StreamName = "astreamname1",
                        Version = 4
                    }
                }));

            this.handler.ProcessingErrors.Should().HaveCount(1);
            this.handler.ProcessingErrors[0].Exception.Should().BeOfType<InvalidOperationException>();
        }
    }

    public class TestEventHandler : EventStreamHandlerBase
    {
        private readonly Mock<Action<string, List<EventStreamStateChangeEvent>>> mock;

        public TestEventHandler(Mock<Action<string, List<EventStreamStateChangeEvent>>> mock) : base(Mock.Of<ILogger>(),
            Mock.Of<IEventStreamStorage<TestAggregateRoot>>())
        {
            this.mock = mock;
        }

        protected override void HandleStreamEvents(string streamName, List<EventStreamStateChangeEvent> eventStream)
        {
            this.mock.Object(streamName, eventStream);
        }
    }
}