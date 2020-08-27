using System;
using Domain.Interfaces.Entities;
using DomainServices;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonsApplication.Storage;
using PersonsDomain;

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
                .Returns((PersonEntity) null);

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEnsureEmailIsUniqueAndNotPersonId_ThenReturnsFalse()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns(new PersonEntity(NullLogger.Instance, new FakeIdentifierFactory("anotherpersonid"),
                    Mock.Of<IEmailService>(), new PersonName("afirstname", "alastname")));

            var result = this.service.EnsureEmailIsUnique("anemailaddress", "apersonid");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEnsureEmailIsUniqueAndMatchesPersonId_ThenReturnsTrue()
        {
            this.storage.Setup(s => s.FindByEmailAddress(It.IsAny<string>()))
                .Returns(new PersonEntity(NullLogger.Instance, new FakeIdentifierFactory("apersonid"),
                    Mock.Of<IEmailService>(), new PersonName("afirstname", "alastname")));

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