using System;
using System.Collections.Generic;
using System.Linq;
using Api.Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Funq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceStack;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class AggregateRootBaseSpec
    {
        private Mock<IDependencyContainer> dependencyContainer;
        private TestAggregateRoot entity;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<TestAggregateRoot>()))
                .Returns("anid".ToIdentifier());
            this.dependencyContainer = new Mock<IDependencyContainer>();
            this.dependencyContainer.Setup(dc => dc.Resolve<ILogger>())
                .Returns(this.logger.Object);
            this.dependencyContainer.Setup(dc => dc.Resolve<IIdentifierFactory>())
                .Returns(this.idFactory.Object);

            this.entity = new TestAggregateRoot(this.logger.Object, this.idFactory.Object);
        }

        [TestMethod]
        public void WhenConstructed_ThenIdentifierIsAssigned()
        {
            this.entity.Id.Should().Be("anid".ToIdentifier());
        }

        [TestMethod]
        public void WhenConstructed_ThenDatesAssigned()
        {
            var now = DateTime.UtcNow;

            this.entity.LastPersistedAtUtc.Should().BeNull();
            this.entity.CreatedAtUtc.Should().BeCloseTo(now);
            this.entity.LastModifiedAtUtc.Should().BeAfter(this.entity.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenConstructed_ThenRaisesEvent()
        {
            this.entity.Events.Count().Should().Be(1);
            this.entity.Events.Should().BeEquivalentTo(new List<object>
            {
                new TestAggregateRoot.CreateEvent
                {
                    APropertyName = "acreatedvalue"
                }
            });
            this.entity.LastModifiedAtUtc.Should().BeAfter(this.entity.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenInstantiateAndCreates_ThenReturnsInstance()
        {
            var result = TestAggregateRoot.Instantiate()(new Dictionary<string, object>
            {
                {nameof(IIdentifiableEntity.Id), "anid".ToIdentifier()}
            }, this.dependencyContainer.Object);

            result.Id.Should().Be("anid".ToIdentifier());
        }

        [TestMethod]
        public void WhenDehydrate_ThenReturnsBaseProperties()
        {
            var now = DateTime.UtcNow;

            var result = this.entity.Dehydrate();

            result.Count.Should().Be(5);
            result[nameof(EntityBase.Id)].Should().Be("anid".ToIdentifier());
            ((DateTime?) result[nameof(EntityBase.LastPersistedAtUtc)]).Should().BeNull();
            ((DateTime) result[nameof(EntityBase.CreatedAtUtc)]).Should().BeCloseTo(now, 500);
            ((DateTime) result[nameof(EntityBase.LastModifiedAtUtc)]).Should().BeCloseTo(now, 500);
            ((List<object>) result[AggregateRootBase.EventsPropertyName]).Single().Should()
                .BeEquivalentTo(new TestAggregateRoot.CreateEvent {APropertyName = "acreatedvalue"});
        }

        [TestMethod]
        public void WhenRehydrate_ThenBasePropertiesHydrated()
        {
            var datum = DateTime.UtcNow.AddYears(1);
            var changes = new List<object>
            {
                new TestAggregateRoot.CreateEvent {APropertyName = "acreatedvalue"}
            };
            var properties = new Dictionary<string, object>
            {
                {nameof(EntityBase.Id), "anid".ToIdentifier()},
                {nameof(EntityBase.LastPersistedAtUtc), datum},
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum},
                {AggregateRootBase.EventsPropertyName, changes}
            };

            this.entity.Rehydrate(properties);

            this.entity.Id.Should().Be("anid".ToIdentifier());
            this.entity.LastPersistedAtUtc.Should().Be(datum);
            this.entity.CreatedAtUtc.Should().Be(datum);
            this.entity.LastModifiedAtUtc.Should().Be(datum);
            this.entity.Events.Count().Should().Be(1);
            this.entity.Events.Should().BeEquivalentTo(changes);
        }

        [TestMethod]
        public void WhenInstantiate_ThenRaisesNoEvents()
        {
            var datum = DateTime.UtcNow.AddYears(1);
            var properties = new Dictionary<string, object>
            {
                {nameof(EntityBase.Id), "anid".ToIdentifier()},
                {nameof(EntityBase.LastPersistedAtUtc), datum},
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum}
            };
            var container = new Container();
            container.AddSingleton<ILogger>(NullLogger.Instance);

            var created = TestAggregateRoot.Instantiate()(properties, new FuncDependencyContainer(container));

            created.Events.Should().BeEmpty();
            created.LastPersistedAtUtc.Should().BeNull();
            created.CreatedAtUtc.Should().Be(DateTime.MinValue);
            created.LastModifiedAtUtc.Should().Be(DateTime.MinValue);
        }

        [TestMethod]
        public void WhenChangeProperty_ThenRaisesEventAndModified()
        {
            this.entity.ChangeProperty("achangedvalue");

            this.entity.Events.Count().Should().Be(2);
            this.entity.Events.Should().BeEquivalentTo(new List<object>
            {
                new TestAggregateRoot.CreateEvent
                {
                    APropertyName = "acreatedvalue"
                },
                new TestAggregateRoot.ChangeEvent
                {
                    APropertyName = "achangedvalue"
                }
            });
            this.entity.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow);
        }
    }
}