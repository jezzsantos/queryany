using System;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonsApplication.ReadModels;
using PersonsApplication.Storage;

namespace PersonsApplication.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class EmailServiceSpec
    {
        private EmailService service;
        private Mock<IPersonStorage> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.storage = new Mock<IPersonStorage>();
            this.service = new EmailService(this.storage.Object);
        }

        [TestMethod]
        public void WhenEnsureEmailIsUniqueAndNoPersons_ThenReturnsTrue()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns((Person) null);

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEnsureEmailIsUniqueAndNotPersonId_ThenReturnsFalse()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns(new Person {Id = "anotherpersonid"});

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeFalse();
        }

        [TestMethod]
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