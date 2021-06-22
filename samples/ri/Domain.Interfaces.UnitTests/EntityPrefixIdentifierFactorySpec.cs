using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class EntityPrefixIdentifierFactorySpec
    {
        private readonly TestEntityPrefixIdentifierFactory factory;

        public EntityPrefixIdentifierFactorySpec()
        {
            this.factory = new TestEntityPrefixIdentifierFactory();
        }

        [Fact]
        public void WhenCreateWithUnknownEntityType_ThenReturnsGuid()
        {
            var result = this.factory.Create(new UnknownEntity());

            result.ToString().Should().MatchRegex(@"xxx_[\d\w]{10,22}");
        }

        [Fact]
        public void WhenCreateWithKnownEntity_ThenReturnsId()
        {
            var result = this.factory.Create(new KnownEntity());

            result.ToString().Should().MatchRegex(@"kno_[\d\w]{10,22}");
        }

        [Fact]
        public void WhenIsValidWithNull_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(null);

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidWithTooShortId_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("tooshort"));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidWithInvalidPrefix_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("999_123456789012"));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidWithTooShortRandomPart_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("xxx_123456789"));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidWithTooLongRandomPart_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("xxx_12345678901234567890123"));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsValidWithUnknownPrefix_ThenReturnsTrue()
        {
            var result = this.factory.IsValid(Identifier.Create("xxx_123456789012"));

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsValidWithKnownPrefix_ThenReturnsTrue()
        {
            var result = this.factory.IsValid(Identifier.Create("kno_123456789012"));

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsValidWithAnonymousUserId_ThenReturnsTrue()
        {
            var result = this.factory.IsValid(CallerConstants.AnonymousUserId.ToIdentifier());

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsValidWithKnownSupportedPrefix_ThenReturnsTrue()
        {
            this.factory.AddSupportedPrefix("another");

            var result = this.factory.IsValid(Identifier.Create("another_123456789012"));

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenConvertGuidWithKnownGuid_ThenReturnsConverted()
        {
            var id = EntityPrefixIdentifierFactory.ConvertGuid(new Guid("65dd0b02-170b-4ea1-a5a5-00d2808b9aee"),
                "known");

            id.Should().Be("known_AgvdZQsXoU6lpQDSgIua7g");
        }
    }

    public class TestEntityPrefixIdentifierFactory : EntityPrefixIdentifierFactory
    {
        public TestEntityPrefixIdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(KnownEntity), "kno"}
        })
        {
        }
    }

    public class KnownEntity : IIdentifiableEntity
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public Identifier Id { get; }
    }

    public class UnknownEntity : IIdentifiableEntity
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public Identifier Id { get; }
    }
}