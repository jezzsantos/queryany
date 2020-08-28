using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonsDomain.Properties;

namespace PersonsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class PersonEntitySpec
    {
        private PersonEntity entity;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;
        private Mock<IEmailService> uniqueEmailService;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("apersonid".ToIdentifier);
            this.uniqueEmailService = new Mock<IEmailService>();
            this.uniqueEmailService.Setup(ues => ues.EnsureEmailIsUnique(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            this.entity = new PersonEntity(this.logger.Object, this.identifierFactory.Object,
                this.uniqueEmailService.Object);
        }

        [TestMethod]
        public void WhenSetName_ThenNameAndDisplayNameAssigned()
        {
            this.entity.SetName(new PersonName("afirstname", "alastname"));

            this.entity.Name.Should().Be(new PersonName("afirstname", "alastname"));
            this.entity.DisplayName.Should().Be(new PersonDisplayName("afirstname"));
            this.entity.Events[1].Should().BeOfType<Events.Person.NameChanged>();
        }

        [TestMethod]
        public void WhenSetDisplayName_ThenEmailAssigned()
        {
            this.entity.SetDisplayName(new PersonDisplayName("adisplayname"));

            this.entity.DisplayName.Should().Be(new PersonDisplayName("adisplayname"));
            this.entity.Events[1].Should().BeOfType<Events.Person.DisplayNameChanged>();
        }

        [TestMethod]
        public void WhenSetEmail_ThenEmailAssigned()
        {
            this.entity.SetEmail(new Email("anemail@company.com"));

            this.entity.Email.Should().Be(new Email("anemail@company.com"));
            this.entity.Events[1].Should().BeOfType<Events.Person.EmailChanged>();
        }

        [TestMethod]
        public void WhenSetEmailAndNotUnique_ThenThrows()
        {
            this.uniqueEmailService.Setup(ues => ues.EnsureEmailIsUnique(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            this.entity
                .Invoking(x =>
                    x.SetEmail(new Email("anemail@company.com")))
                .Should().Throw<RuleViolationException>(Resources.PersonEntity_EmailNotUnique);
        }

        [TestMethod]
        public void WhenSetPhoneNumber_ThenEmailAssigned()
        {
            this.entity.SetPhoneNumber(new PhoneNumber("+64277888111"));

            this.entity.Phone.Should().Be(new PhoneNumber("+64277888111"));
            this.entity.Events[1].Should().BeOfType<Events.Person.PhoneNumberChanged>();
        }
    }
}