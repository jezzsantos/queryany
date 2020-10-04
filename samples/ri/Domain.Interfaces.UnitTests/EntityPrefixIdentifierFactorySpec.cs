using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class EntityPrefixIdentifierFactorySpec
    {
        private TestEntityPrefixIdentifierFactory factory;

        [TestInitialize]
        public void Initialize()
        {
            this.factory = new TestEntityPrefixIdentifierFactory();
        }

        [TestMethod]
        public void WhenCreateWithUnknownEntityType_ThenReturnsGuid()
        {
            var result = this.factory.Create(new UnknownEntity());

            result.ToString().Should().MatchRegex(@"xxx_[\d\w]{10,22}");
        }

        [TestMethod]
        public void WhenCreateWithKnownEntity_ThenReturnsId()
        {
            var result = this.factory.Create(new KnownEntity());

            result.ToString().Should().MatchRegex(@"kno_[\d\w]{10,22}");
        }

        [TestMethod]
        public void WhenIsValidWithNull_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(null);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIsValidWithTooShortId_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("tooshort"));

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIsValidWithInvalidPrefix_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("999_123456789012"));

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIsValidWithTooShortRandomPart_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("xxx_123456789"));

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIsValidWithTooLongRandomPart_ThenReturnsFalse()
        {
            var result = this.factory.IsValid(Identifier.Create("xxx_12345678901234567890123"));

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenIsValidWithUnknownPrefix_ThenReturnsTrue()
        {
            var result = this.factory.IsValid(Identifier.Create("xxx_123456789012"));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenIsValidWithKnownPrefix_ThenReturnsTrue()
        {
            var result = this.factory.IsValid(Identifier.Create("kno_123456789012"));

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenIsValidWithAnonymousUserId_ThenReturnsTrue()
        {
            var result = this.factory.IsValid(CurrentCallerConstants.AnonymousUserId.ToIdentifier());

            result.Should().BeTrue();
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