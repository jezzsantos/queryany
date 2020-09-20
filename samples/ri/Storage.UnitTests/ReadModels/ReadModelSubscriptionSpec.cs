using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Storage.Interfaces;
using Storage.Interfaces.ReadModels;
using Storage.ReadModels;

namespace Storage.UnitTests.ReadModels
{
    [TestClass, TestCategory("Unit")]
    public class ReadModelSubscriptionSpec
    {
        private Mock<IEventStreamStorage<TestAggregateRoot>> eventingStorage;
        private Mock<ILogger> logger;
        private Mock<IReadModelProjector> readModelStorage;
        private InProcessReadModelSubscription subscription;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.eventingStorage = new Mock<IEventStreamStorage<TestAggregateRoot>>();
            this.readModelStorage = new Mock<IReadModelProjector>();
            this.subscription = new InProcessReadModelSubscription(this.logger.Object, this.readModelStorage.Object,
                this.eventingStorage.Object);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndNoEvents_ThenDoesNotWriteEvents()
        {
            this.subscription.OnEventStreamStateChanged(null,
                new EventStreamStateChangedArgs(new List<EventStreamStateChangeEvent>()));

            this.readModelStorage.Verify(
                rms => rms.WriteEventStream("astreamname", It.IsAny<List<EventStreamStateChangeEvent>>()), Times.Never);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndFromDifferentStreams_ThenWritesBatchedEvents()
        {
            this.subscription.OnEventStreamStateChanged(null,
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

            this.readModelStorage.Verify(
                rms => rms.WriteEventStream("astreamname1", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 2
                    && batch[0].Id == "aneventid3"
                    && batch[1].Id == "aneventid1"
                )), Times.Once);
            this.readModelStorage.Verify(
                rms => rms.WriteEventStream("astreamname2", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 1
                    && batch[0].Id == "aneventid2"
                )), Times.Once);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndFromDifferentStreamsAndWriteFails_ThenWritesRemainingBatches()
        {
            this.readModelStorage.Setup(rms =>
                    rms.WriteEventStream(It.IsAny<string>(), It.IsAny<List<EventStreamStateChangeEvent>>()))
                .Throws<Exception>();

            this.subscription.OnEventStreamStateChanged(null,
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

            this.readModelStorage.Verify(
                rms => rms.WriteEventStream("astreamname1", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 2
                    && batch[0].Id == "aneventid3"
                    && batch[1].Id == "aneventid1"
                )), Times.Once);
            this.readModelStorage.Verify(
                rms => rms.WriteEventStream("astreamname2", It.Is<List<EventStreamStateChangeEvent>>(batch =>
                    batch.Count() == 1
                    && batch[0].Id == "aneventid2"
                )), Times.Once);
        }

        [TestMethod]
        public void WhenEventStreamChangedEventRaisedAndEventsAreOutOfOrder_ThenThrows()
        {
            this.subscription.OnEventStreamStateChanged(null,
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

            this.subscription.ProcessingErrors.Should().HaveCount(1);
            this.subscription.ProcessingErrors[0].Exception.Should().BeOfType<InvalidOperationException>();
        }
    }
}