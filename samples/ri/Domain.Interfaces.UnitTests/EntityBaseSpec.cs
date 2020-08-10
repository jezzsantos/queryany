using System;
using System.Collections.Generic;
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
        public void WhenGetFactoryAndCreates_ThenReturnsIdentifiedInstance()
        {
            var factory = TestEntity.GetFactory(this.logger.Object);

            var result = factory(new Dictionary<string, object>
            {
                {nameof(IIdentifiableEntity.Id), "anid".ToIdentifier()}
            });

            result.Id.Should().Be("anid".ToIdentifier());
        }

        [TestMethod]
        public void WhenDehydrate_ThenReturnsBaseProperties()
        {
            var now = DateTime.UtcNow;

            var result = this.entity.Dehydrate();

            result.Count.Should().Be(3);
            result[nameof(EntityBase.Id)].Should().Be("anid".ToIdentifier());
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
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum}
            };

            this.entity.Rehydrate(properties);

            this.entity.Id.Should().Be("anid".ToIdentifier());
            this.entity.CreatedAtUtc.Should().Be(datum);
            this.entity.LastModifiedAtUtc.Should().Be(datum);
        }
    }
}