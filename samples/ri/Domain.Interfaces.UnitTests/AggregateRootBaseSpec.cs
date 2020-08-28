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
        private TestAggregateRoot aggregate;
        private Mock<IDependencyContainer> dependencyContainer;
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
        public void WhenConstructed_ThenRaisesEvent()
        {
            this.aggregate.Events.Count().Should().Be(1);
            this.aggregate.Events[0].Should().BeOfType<Events.Any.Created>();
            this.aggregate.LastModifiedAtUtc.Should().BeAfter(this.aggregate.CreatedAtUtc);
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

            var result = this.aggregate.Dehydrate();

            result.Count.Should().Be(4);
            result[nameof(EntityBase.Id)].Should().Be("anid".ToIdentifier());
            ((DateTime?) result[nameof(EntityBase.LastPersistedAtUtc)]).Should().BeNull();
            ((DateTime) result[nameof(EntityBase.CreatedAtUtc)]).Should().BeCloseTo(now, 500);
            ((DateTime) result[nameof(EntityBase.LastModifiedAtUtc)]).Should().BeCloseTo(now, 500);
        }

        [TestMethod]
        public void WhenRehydrate_ThenBasePropertiesHydrated()
        {
            var datum = DateTime.UtcNow.AddYears(1);
            var properties = new Dictionary<string, object>
            {
                {nameof(EntityBase.Id), "anid".ToIdentifier()},
                {nameof(EntityBase.LastPersistedAtUtc), datum},
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum}
            };

            this.aggregate.Rehydrate(properties);

            this.aggregate.Id.Should().Be("anid".ToIdentifier());
            this.aggregate.LastPersistedAtUtc.Should().Be(datum);
            this.aggregate.CreatedAtUtc.Should().Be(datum);
            this.aggregate.LastModifiedAtUtc.Should().Be(datum);
            this.aggregate.Events.Count().Should().Be(1);
            this.aggregate.Events[0].Should().BeOfType<Events.Any.Created>();
        }

        [TestMethod]
        public void WhenInstantiate_ThenRaisesNoEvents()
        {
            var container = new Container();
            container.AddSingleton<ILogger>(NullLogger.Instance);
            container.AddSingleton<IIdentifierFactory>(new NullIdentifierFactory());

            var created =
                TestAggregateRoot.Instantiate()("anid".ToIdentifier(), new FuncDependencyContainer(container));

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
            this.aggregate.Events[0].Should().BeOfType<Events.Any.Created>();
            this.aggregate.Events[1].Should().BeEquivalentTo(new TestAggregateRoot.ChangeEvent
                {APropertyName = "achangedvalue"});
            this.aggregate.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow);
        }
    }
}