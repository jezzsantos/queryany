using System;
using System.Collections.Generic;
using ApplicationServices;
using Domain.Interfaces.Entities;
using FluentAssertions;
using InfrastructureServices.Eventing.Notifications;
using InfrastructureServices.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Storage.Interfaces;

namespace InfrastructureServices.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class DomainEventNotificationProducerSpec
    {
        private ChangeEventTypeMigrator changeEventTypeMigrator;
        private Mock<ILogger> logger;
        private DomainEventNotificationProducer notificationProducer;
        private Mock<IDomainEventPublisherSubscriberPair> pair;
        private List<IDomainEventPublisherSubscriberPair> pairs;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.changeEventTypeMigrator = new ChangeEventTypeMigrator();
            this.pair = new Mock<IDomainEventPublisherSubscriberPair>();
            this.pair.Setup(p => p.Publisher.EntityType)
                .Returns(typeof(string));
            this.pair.Setup(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()))
                .Returns((IChangeEvent) null);
            this.pair.Setup(p => p.Subscriber.Notify(It.IsAny<IChangeEvent>()))
                .Returns(true);
            this.pairs = new List<IDomainEventPublisherSubscriberPair> {this.pair.Object};
            this.notificationProducer = new DomainEventNotificationProducer(this.logger.Object,
                this.changeEventTypeMigrator,
                this.pairs.ToArray());
        }

        [TestMethod]
        public void WhenWriteEventStreamAndNoEvents_ThenReturns()
        {
            this.notificationProducer.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>());

            this.pair.Verify(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenWriteEventStreamAndNoConfiguredSubscriber_ThenThrows()
        {
            this.pair.Setup(p => p.Publisher.EntityType)
                .Returns(typeof(string));
            this.notificationProducer
                .Invoking(x => x.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        EntityType = "atypename"
                    }
                })).Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.DomainEventNotificationProducer_UnexpectedError);

            this.pair.Verify(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenWriteEventStreamAndDeserializationOfEventsFails_ThenThrows()
        {
            this.notificationProducer
                .Invoking(x => x.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        Id = "anid",
                        EntityType = nameof(String),
                        Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid"}),
                        Version = 1,
                        Metadata = new EventMetadata("unknowntype")
                    }
                })).Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.DomainEventNotificationProducer_UnexpectedError);
        }

        [TestMethod]
        public void WhenWriteEventStreamAndFirstEverEvent_ThenNotifiesEvents()
        {
            this.pair.Setup(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()))
                .Returns((IChangeEvent @event) => new TestChangeEvent {EntityId = @event.EntityId});

            this.notificationProducer.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
            {
                new EventStreamStateChangeEvent
                {
                    Id = "anid1",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid1"}),
                    Version = 0,
                    Metadata = new EventMetadata(typeof(TestChangeEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid2"}),
                    Version = 1,
                    Metadata = new EventMetadata(typeof(TestChangeEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid3"}),
                    Version = 2,
                    Metadata = new EventMetadata(typeof(TestChangeEvent).AssemblyQualifiedName)
                }
            });

            this.pair.Verify(p => p.Publisher.Publish(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.pair.Verify(p => p.Subscriber.Notify(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.pair.Verify(p => p.Publisher.Publish(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid3"
            )));
            this.pair.Verify(p => p.Subscriber.Notify(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid3"
            )));
        }

        [TestMethod]
        public void WhenWriteEventStream_ThenNotifiesEvents()
        {
            this.pair.Setup(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()))
                .Returns((IChangeEvent @event) => new TestChangeEvent {EntityId = @event.EntityId});

            this.notificationProducer.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>
            {
                new EventStreamStateChangeEvent
                {
                    Id = "anid1",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid1"}),
                    Version = 3,
                    Metadata = new EventMetadata(typeof(TestChangeEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid2",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid2"}),
                    Version = 4,
                    Metadata = new EventMetadata(typeof(TestChangeEvent).AssemblyQualifiedName)
                },
                new EventStreamStateChangeEvent
                {
                    Id = "anid3",
                    EntityType = nameof(String),
                    Data = EntityEvent.ToData(new TestChangeEvent {EntityId = "aneventid3"}),
                    Version = 5,
                    Metadata = new EventMetadata(typeof(TestChangeEvent).AssemblyQualifiedName)
                }
            });

            this.pair.Verify(p => p.Publisher.Publish(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid1"
            )));
            this.pair.Verify(p => p.Subscriber.Notify(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid1"
            )));
            this.pair.Verify(p => p.Publisher.Publish(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.pair.Verify(p => p.Subscriber.Notify(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid2"
            )));
            this.pair.Verify(p => p.Publisher.Publish(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid3"
            )));
            this.pair.Verify(p => p.Subscriber.Notify(It.Is<TestChangeEvent>(e =>
                e.EntityId == "aneventid3"
            )));
        }
    }
}