using System;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

            this.entity.LastPersistedAtUtc.Should().BeNull();
            this.entity.CreatedAtUtc.Should().BeCloseTo(now);
            this.entity.LastModifiedAtUtc.Should().Be(this.entity.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenChangeProperty_ThenModified()
        {
            this.entity.ChangeProperty("avalue");

            this.entity.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow);
        }

        [TestMethod]
        public void WhenChangePropertyAndSetAggregateEventHandler_ThenEventHandledByHandler()
        {
            object handledAggregateEvent = null;
            this.entity.SetAggregateEventHandler(o => { handledAggregateEvent = o; });

            this.entity.ChangeProperty("avalue");

            handledAggregateEvent.Should().BeEquivalentTo(new TestEntity.ChangeEvent
            {
                APropertyName = "avalue"
            });
            this.entity.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow);
        }
    }
}