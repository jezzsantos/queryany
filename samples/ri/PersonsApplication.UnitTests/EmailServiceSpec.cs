using System;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using PersonsApplication.ReadModels;
using PersonsApplication.Storage;
using Xunit;

namespace PersonsApplication.UnitTests
{
    [Trait("Category", "Unit")]
    public class EmailServiceSpec
    {
        private readonly EmailService service;
        private readonly Mock<IPersonStorage> storage;

        public EmailServiceSpec()
        {
            this.storage = new Mock<IPersonStorage>();
            this.service = new EmailService(this.storage.Object);
        }

        [Fact]
        public void WhenEnsureEmailIsUniqueAndNoPersons_ThenReturnsTrue()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns((Person) null);

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenEnsureEmailIsUniqueAndNotPersonId_ThenReturnsFalse()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns(new Person {Id = "anotherpersonid"});

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEnsureEmailIsUniqueAndMatchesPersonId_ThenReturnsTrue()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns(new Person {Id = "apersonid"});

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeTrue();
        }
    }

    public class FakeIdentifierFactory : IIdentifierFactory
    {
        private readonly string id;

        public FakeIdentifierFactory(string id)
        {
            this.id = id;
        }

        public Identifier Create(IIdentifiableEntity entity)
        {
            return this.id.ToIdentifier();
        }

        public bool IsValid(Identifier value)
        {
            throw new NotImplementedException();
        }
    }
}