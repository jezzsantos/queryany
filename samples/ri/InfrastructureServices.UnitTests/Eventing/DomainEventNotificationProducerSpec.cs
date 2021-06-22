using System;
using System.Collections.Generic;
using Application.Storage.Interfaces;
using ApplicationServices.Interfaces;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using InfrastructureServices.Eventing.Notifications;
using InfrastructureServices.Properties;
using Moq;
using UnitTesting.Common;
using Xunit;

namespace InfrastructureServices.UnitTests.Eventing
{
    [Trait("Category", "Unit")]
    public class DomainEventNotificationProducerSpec
    {
        private readonly DomainEventNotificationProducer notificationProducer;
        private readonly Mock<IDomainEventPublisherSubscriberPair> pair;

        public DomainEventNotificationProducerSpec()
        {
            var recorder = new Mock<IRecorder>();
            var changeEventTypeMigrator = new ChangeEventTypeMigrator();
            this.pair = new Mock<IDomainEventPublisherSubscriberPair>();
            this.pair.Setup(p => p.Publisher.EntityType)
                .Returns(typeof(string));
            this.pair.Setup(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()))
                .Returns((IChangeEvent) null);
            this.pair.Setup(p => p.Subscriber.Notify(It.IsAny<IChangeEvent>()))
                .Returns(true);
            var pairs = new List<IDomainEventPublisherSubscriberPair> {this.pair.Object};
            this.notificationProducer = new DomainEventNotificationProducer(recorder.Object,
                changeEventTypeMigrator,
                pairs.ToArray());
        }

        [Fact]
        public void WhenWriteEventStreamAndNoEvents_ThenReturns()
        {
            this.notificationProducer.WriteEventStream("astreamname", new List<EventStreamStateChangeEvent>());

            this.pair.Verify(p => p.Publisher.Publish(It.IsAny<IChangeEvent>()), Times.Never);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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