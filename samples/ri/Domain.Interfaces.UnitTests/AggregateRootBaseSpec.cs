using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class AggregateRootBaseSpec
    {
        private TestAggregateRoot aggregate;
        private Mock<IDependencyContainer> dependencyContainer;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;
        private ChangeEventTypeMigrator typeMigrator;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier());
            this.dependencyContainer = new Mock<IDependencyContainer>();
            this.dependencyContainer.Setup(dc => dc.Resolve<ILogger>())
                .Returns(this.logger.Object);
            this.dependencyContainer.Setup(dc => dc.Resolve<IIdentifierFactory>())
                .Returns(this.idFactory.Object);
            this.typeMigrator = new ChangeEventTypeMigrator();

            this.aggregate = new TestAggregateRoot(this.logger.Object, this.idFactory.Object);
        }

        [TestMethod]
        public void WhenConstructed_ThenIdentifierIsAssigned()
        {
            this.aggregate.Id.Should().Be("anid".ToIdentifier());
        }

        [TestMethod]
        public void WhenConstructed_ThenDatesAssigned()
        {
            var now = DateTime.UtcNow;

            this.aggregate.LastPersistedAtUtc.Should().BeNull();
            this.aggregate.CreatedAtUtc.Should().BeCloseTo(now);
            this.aggregate.LastModifiedAtUtc.Should().BeAfter(this.aggregate.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenConstructed_ThenChangeVersion0()
        {
            this.aggregate.ChangeVersion.Should().Be(0);
        }

        [TestMethod]
        public void WhenConstructed_ThenRaisesEvent()
        {
            this.aggregate.Events.Count().Should().Be(1);
            this.aggregate.Events[0].Should().BeOfType<TestAggregateRoot.CreateEvent>();
            this.aggregate.LastModifiedAtUtc.Should().BeAfter(this.aggregate.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenInstantiateAndCreates_ThenReturnsInstance()
        {
            var result = TestAggregateRoot.Instantiate()("anid".ToIdentifier(), this.dependencyContainer.Object,
                new Dictionary<string, object>());

            result.Id.Should().Be("anid".ToIdentifier());
        }

        [TestMethod]
        public void WhenInstantiate_ThenRaisesNoEvents()
        {
            var container = new Mock<IDependencyContainer>();
            container.Setup(c => c.Resolve<ILogger>())
                .Returns(NullLogger.Instance);
            container.Setup(c => c.Resolve<IIdentifierFactory>())
                .Returns(new NullIdentifierFactory());

            var created =
                TestAggregateRoot.Instantiate()("anid".ToIdentifier(), container.Object,
                    new Dictionary<string, object>());

            created.GetChanges().Should().BeEmpty();
            created.LastPersistedAtUtc.Should().BeNull();
            created.CreatedAtUtc.Should().Be(DateTime.MinValue);
            created.LastModifiedAtUtc.Should().Be(DateTime.MinValue);
        }

        [TestMethod]
        public void WhenChangeProperty_ThenRaisesEventAndModified()
        {
            this.aggregate.ChangeProperty("achangedvalue");

            this.aggregate.Events.Count().Should().Be(2);
            this.aggregate.Events[0].Should().BeOfType<TestAggregateRoot.CreateEvent>();
            this.aggregate.Events[1].Should().BeEquivalentTo(new TestAggregateRoot.ChangeEvent
                {APropertyName = "achangedvalue"});
            this.aggregate.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, 50);
        }

        [TestMethod]
        public void WhenGetChanges_ThenReturnsEventEntities()
        {
            this.aggregate.ChangeProperty("avalue1");

            var result = this.aggregate.GetChanges();

            result.Count.Should().Be(2);
            result[0].EventType.Should().Be(nameof(TestAggregateRoot.CreateEvent));
            result[0].StreamName.Should().Be("testaggregateroot_anid");
            result[0].Version.Should().Be(1);
            result[0].Metadata.Fqn.Should().Be(typeof(TestAggregateRoot.CreateEvent).AssemblyQualifiedName);
            result[1].EventType.Should().Be(nameof(TestAggregateRoot.ChangeEvent));
            result[1].StreamName.Should().Be("testaggregateroot_anid");
            result[1].Version.Should().Be(2);
            result[1].Metadata.Fqn.Should().Be(typeof(TestAggregateRoot.ChangeEvent).AssemblyQualifiedName);
        }

        [TestMethod]
        public void WhenLoadChangesAgainAndEventVersionsMismatched_ThenThrows()
        {
            ((IPersistableAggregateRoot) this.aggregate).LoadChanges(new List<EntityEvent>
            {
                CreateEventEntity("aneventid1", 1)
            }, this.typeMigrator);

            ((IPersistableAggregateRoot) this.aggregate)
                .Invoking(x => x.LoadChanges(new List<EntityEvent>
                {
                    CreateEventEntity("aneventid1", 1),
                    CreateEventEntity("aneventid2", 2),
                    CreateEventEntity("aneventid3", 3)
                }, this.typeMigrator))
                .Should()
                .Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void WhenLoadChangesAgain_ThenAppendsEvents()
        {
            ((IPersistableAggregateRoot) this.aggregate).LoadChanges(new List<EntityEvent>
            {
                CreateEventEntity("aneventid1", 1)
            }, this.typeMigrator);

            ((IPersistableAggregateRoot) this.aggregate).LoadChanges(new List<EntityEvent>
            {
                CreateEventEntity("aneventid2", 2),
                CreateEventEntity("aneventid3", 3),
                CreateEventEntity("aneventid4", 4)
            }, this.typeMigrator);

            this.aggregate.ChangeVersion.Should().Be(4);
        }

        [TestMethod]
        public void WhenLoadChanges_ThenSetsEventsAndUpdatesVersion()
        {
            ((IPersistableAggregateRoot) this.aggregate).LoadChanges(new List<EntityEvent>
            {
                CreateEventEntity("aneventid1", 1),
                CreateEventEntity("aneventid2", 2),
                CreateEventEntity("aneventid3", 3)
            }, this.typeMigrator);

            this.aggregate.ChangeVersion.Should().Be(3);
        }

        [TestMethod]
        public void WhenToEventAfterGetChanges_ThenReturnsOriginalEvent()
        {
            this.aggregate.ChangeProperty("avalue");

            var entities = this.aggregate.GetChanges();

            var created = entities[0].ToEvent(this.typeMigrator);

            created.Should().BeOfType<TestAggregateRoot.CreateEvent>();
            created.As<TestAggregateRoot.CreateEvent>().EntityId.Should().Be("anid");

            var changed = entities[1].ToEvent(this.typeMigrator);

            changed.Should().BeOfType<TestAggregateRoot.ChangeEvent>();
            changed.As<TestAggregateRoot.ChangeEvent>().APropertyName.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenClearChanges_ThenResetsLastPersisted()
        {
            this.aggregate.ClearChanges();

            this.aggregate.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow);
        }

        private static EntityEvent CreateEventEntity(string id, long version)
        {
            var entity = new EntityEvent();
            entity.SetIdentifier(new FixedIdentifierFactory(id));
            entity.SetEvent("astreamname", "anentitytype", version, new TestEvent {APropertyValue = "avalue"});

            return entity;
        }
    }
}