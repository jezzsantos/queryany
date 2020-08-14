using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PersonsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class PersonEntitySpec
    {
        private PersonEntity entity;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.entity = new PersonEntity(this.logger.Object, this.identifierFactory.Object,
                new PersonName("afirstname", "alastname"));
        }

        [TestMethod]
        public void WhenSetEmail_ThenEmailAssigned()
        {
            this.entity.SetEmail(new Email("anemail@company.com"));

            var result = this.entity.Email;

            result.Should().Be(new Email("anemail@company.com"));
        }

        [TestMethod]
        public void WhenSetPhoneNumber_ThenEmailAssigned()
        {
            this.entity.SetPhoneNumber(new PhoneNumber("+64277888111"));

            var result = this.entity.Phone;

            result.Should().Be(new PhoneNumber("+64277888111"));
        }
    }
}