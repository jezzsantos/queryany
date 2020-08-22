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
    public class EntityBaseSpec
    {
        private Mock<IDependencyContainer> dependencyContainer;
        private TestEntity entity;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<TestEntity>()))
                .Returns("anid".ToIdentifier());
            this.dependencyContainer = new Mock<IDependencyContainer>();
            this.dependencyContainer.Setup(dc => dc.Resolve<ILogger>())
                .Returns(this.logger.Object);
            this.dependencyContainer.Setup(dc => dc.Resolve<IIdentifierFactory>())
                .Returns(this.idFactory.Object);

            this.entity = new TestEntity(this.logger.Object, this.idFactory.Object);
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

            this.entity.CreatedAtUtc.Should().BeCloseTo(now);
            this.entity.LastModifiedAtUtc.Should().Be(this.entity.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenConstructed_ThenHasChanges()
        {
            var changes = new List<object>
            {
                new TestEntity.CreateEvent
                {
                    APropertyName = "acreatedvalue"
                }
            };

            this.entity.Events.Count().Should().Be(1);
            this.entity.Events.Should().BeEquivalentTo(changes);
        }

        [TestMethod]
        public void WhenInstantiateAndCreates_ThenReturnsIdentifiedInstance()
        {
            var result = TestEntity.Instantiate()(new Dictionary<string, object>
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

            result.Count.Should().Be(4);
            result[nameof(EntityBase.Id)].Should().Be("anid".ToIdentifier());
            ((DateTime) result[nameof(EntityBase.CreatedAtUtc)]).Should().BeCloseTo(now, 500);
            ((DateTime) result[nameof(EntityBase.LastModifiedAtUtc)]).Should().BeCloseTo(now, 500);
            ((List<object>) result[EntityBase.EventsPropertyName]).Single().Should()
                .BeEquivalentTo(new TestEntity.CreateEvent {APropertyName = "acreatedvalue"});
        }

        [TestMethod]
        public void WhenRehydrate_ThenBasePropertiesHydrated()
        {
            var datum = DateTime.UtcNow.AddYears(1);
            var changes = new List<object>
            {
                new TestEntity.CreateEvent {APropertyName = "acreatedvalue"}
            };
            var properties = new Dictionary<string, object>
            {
                {nameof(EntityBase.Id), "anid".ToIdentifier()},
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum},
                {EntityBase.EventsPropertyName, changes}
            };

            this.entity.Rehydrate(properties);

            this.entity.Id.Should().Be("anid".ToIdentifier());
            this.entity.CreatedAtUtc.Should().Be(datum);
            this.entity.LastModifiedAtUtc.Should().Be(datum);
            this.entity.Events.Count().Should().Be(1);
            this.entity.Events.Should().BeEquivalentTo(changes);
        }

        [TestMethod]
        public void WhenInstantiate_ThenChangesEmpty()
        {
            var datum = DateTime.UtcNow.AddYears(1);
            var properties = new Dictionary<string, object>
            {
                {nameof(EntityBase.Id), "anid".ToIdentifier()},
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum}
            };
            var container = new Container();
            container.AddSingleton<ILogger>(NullLogger.Instance);

            var created = TestEntity.Instantiate()(properties, new FuncDependencyContainer(container));

            created.Events.Should().BeEmpty();
        }

        [TestMethod]
        public void WhenChangeProperty_ThenChanged()
        {
            this.entity.ChangeProperty("achangedvalue");

            this.entity.Events.Count().Should().Be(2);
            this.entity.Events.Should().BeEquivalentTo(new List<object>
            {
                new TestEntity.CreateEvent
                {
                    APropertyName = "acreatedvalue"
                },
                new TestEntity.ChangeEvent
                {
                    APropertyName = "achangedvalue"
                }
            });
        }
    }
}