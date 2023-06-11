using System;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using UnitTesting.Common;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class EntityBaseSpec
    {
        private readonly TestEntity entity;

        public EntityBaseSpec()
        {
            var recorder = new Mock<IRecorder>();
            var idFactory = new Mock<IIdentifierFactory>();
            idFactory.Setup(idf => idf.Create(It.IsAny<TestEntity>()))
                .Returns("anid".ToIdentifier());
            var dependencyContainer = new Mock<IDependencyContainer>();
            dependencyContainer.Setup(dc => dc.Resolve<IRecorder>())
                .Returns(recorder.Object);
            dependencyContainer.Setup(dc => dc.Resolve<IIdentifierFactory>())
                .Returns(idFactory.Object);

            this.entity = new TestEntity(recorder.Object, idFactory.Object);
        }

        [Fact]
        public void WhenConstructed_ThenIdentifierIsAssigned()
        {
            this.entity.Id.Should().Be("anid".ToIdentifier());
        }

        [Fact]
        public void WhenConstructed_ThenDatesAssigned()
        {
            var now = DateTime.UtcNow;

            this.entity.LastPersistedAtUtc.Should().BeNull();
            this.entity.CreatedAtUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
            this.entity.LastModifiedAtUtc.Should().Be(this.entity.CreatedAtUtc);
        }

        [Fact]
        public void WhenChangeProperty_ThenModified()
        {
            this.entity.ChangeProperty("avalue");

            this.entity.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void WhenChangePropertyAndSetAggregateEventHandler_ThenEventHandledByHandler()
        {
            object handledAggregateEvent = null;
            this.entity.SetAggregateEventHandler(o => { handledAggregateEvent = o; });

            this.entity.ChangeProperty("avalue");

            handledAggregateEvent.Should().BeEquivalentTo(new TestEntity.ChangeEvent
            {
                APropertyName = "avalue"
            });
            this.entity.LastModifiedAtUtc.Should().BeNear(DateTime.UtcNow);
        }
    }
}