using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class EntityBaseSpec
    {
        private TestEntity entity;

        [TestInitialize]
        public void Initialize()
        {
            this.entity = new TestEntity();
        }

        [TestMethod]
        public void WhenConstructed_ThenIdentifierIsNull()
        {
            this.entity.Id.Should().Be(null);
        }

        [TestMethod]
        public void WhenIdentifyWithNull_ThenThrows()
        {
            this.entity.Invoking(x => x.Identify(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenIdentify_ThenIdentityAssigned()
        {
            this.entity.Identify(Identifier.Create("anid"));

            this.entity.Id.Should().Be(Identifier.Create("anid"));
        }

        [TestMethod]
        public void WhenIdentifyAndAlreadyIdentified_ThenThrows()
        {
            this.entity.Identify(Identifier.Create("anid"));

            this.entity.Invoking(x => x.Identify(Identifier.Create("anotherid")))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Properties.Resources.EntityBase_IdentifierExists);
        }

        [TestMethod]
        public void WhenUnidentifiedAndEqualWithNull_ThenReturnsFalse()
        {
            var result = this.entity.Equals(null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenUnidentifiedAndEqualWithOtherUnidentifiedEntity_ThenReturnsFalse()
        {
            var otherEntity = new TestEntity();

            var result = this.entity.Equals(otherEntity);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenUnidentifiedAndEqualWithOtherIdentifiedEntity_ThenReturnsFalse()
        {
            var otherEntity = new TestEntity();
            otherEntity.Identify(Identifier.Create("anotherid"));

            var result = this.entity.Equals(otherEntity);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIdentifiedAndEqualWithOtherIdentifiedEntity_ThenReturnsFalse()
        {
            var otherEntity = new TestEntity();
            otherEntity.Identify(Identifier.Create("anotherid"));

            this.entity.Identify(Identifier.Create("anid"));
            var result = this.entity.Equals(otherEntity);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIdentifiedAndEqualWithSameIdentifiedEntity_ThenReturnsTrue()
        {
            var otherEntity = new TestEntity();
            otherEntity.Identify(Identifier.Create("anid"));

            this.entity.Identify(Identifier.Create("anid"));
            var result = this.entity.Equals(otherEntity);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenIdentifiedAndEqualWithSelf_ThenReturnsTrue()
        {
            this.entity.Identify(Identifier.Create("anid"));

            // ReSharper disable once EqualExpressionComparison
            var result = this.entity.Equals(this.entity);

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenDehydrateAndNoIdentifier_ThenReturnsBaseProperties()
        {
            var now = DateTime.UtcNow;

            var result = this.entity.Dehydrate();

            result.Count.Should().Be(3);
            result[nameof(EntityBase.Id)].Should().BeNull();
            ((DateTime) result[nameof(EntityBase.CreatedAtUtc)]).Should().BeCloseTo(now, 500);
            ((DateTime) result[nameof(EntityBase.LastModifiedAtUtc)]).Should().BeCloseTo(now, 500);
        }

        [TestMethod]
        public void WhenDehydrate_ThenReturnsBaseProperties()
        {
            var now = DateTime.UtcNow;
            this.entity.Identify(Identifier.Create("anid"));

            var result = this.entity.Dehydrate();

            result.Count.Should().Be(3);
            result[nameof(EntityBase.Id)].Should().Be("anid");
            ((DateTime) result[nameof(EntityBase.CreatedAtUtc)]).Should().BeCloseTo(now, 500);
            ((DateTime) result[nameof(EntityBase.LastModifiedAtUtc)]).Should().BeCloseTo(now, 500);
        }

        [TestMethod]
        public void WhenRehydrate_ThenBasePropertiesHydrated()
        {
            var datum = DateTime.UtcNow.AddYears(1);
            var properties = new Dictionary<string, object>
            {
                {nameof(EntityBase.Id), "anid"},
                {nameof(EntityBase.CreatedAtUtc), datum},
                {nameof(EntityBase.LastModifiedAtUtc), datum}
            };

            this.entity.Rehydrate(properties);

            this.entity.Id.Should().Be(Identifier.Create("anid"));
            this.entity.CreatedAtUtc.Should().Be(datum);
            this.entity.LastModifiedAtUtc.Should().Be(datum);
        }
    }
}