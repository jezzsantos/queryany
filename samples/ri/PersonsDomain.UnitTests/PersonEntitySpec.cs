using Common;
using Domain.Interfaces.Entities;
using DomainServices.Interfaces;
using FluentAssertions;
using Moq;
using PersonsDomain.Properties;
using Xunit;

namespace PersonsDomain.UnitTests
{
    [Trait("Category", "Unit")]
    public class PersonEntitySpec
    {
        private readonly PersonEntity entity;
        private readonly Mock<IEmailService> uniqueEmailService;

        public PersonEntitySpec()
        {
            var recorder = new Mock<IRecorder>();
            var identifierFactory = new Mock<IIdentifierFactory>();
            identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("apersonid".ToIdentifier);
            this.uniqueEmailService = new Mock<IEmailService>();
            this.uniqueEmailService.Setup(ues => ues.EnsureEmailIsUnique(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            this.entity = new PersonEntity(recorder.Object, identifierFactory.Object,
                this.uniqueEmailService.Object);
        }

        [Fact]
        public void WhenSetName_ThenNameAndDisplayNameAssigned()
        {
            this.entity.SetName(new PersonName("afirstname", "alastname"));

            this.entity.Name.Should().Be(new PersonName("afirstname", "alastname"));
            this.entity.DisplayName.Should().Be(new PersonDisplayName("afirstname"));
            this.entity.Events[1].Should().BeOfType<Events.Person.NameChanged>();
        }

        [Fact]
        public void WhenSetDisplayName_ThenEmailAssigned()
        {
            this.entity.SetDisplayName(new PersonDisplayName("adisplayname"));

            this.entity.DisplayName.Should().Be(new PersonDisplayName("adisplayname"));
            this.entity.Events[1].Should().BeOfType<Events.Person.DisplayNameChanged>();
        }

        [Fact]
        public void WhenSetEmail_ThenEmailAssigned()
        {
            this.entity.SetEmail(new Email("anemail@company.com"));

            this.entity.Email.Should().Be(new Email("anemail@company.com"));
            this.entity.Events[1].Should().BeOfType<Events.Person.EmailChanged>();
        }

        [Fact]
        public void WhenSetEmailAndNotUnique_ThenThrows()
        {
            this.uniqueEmailService.Setup(ues => ues.EnsureEmailIsUnique(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            this.entity
                .Invoking(x =>
                    x.SetEmail(new Email("anemail@company.com")))
                .Should().Throw<RuleViolationException>(Resources.PersonEntity_EmailNotUnique);
        }

        [Fact]
        public void WhenSetPhoneNumber_ThenEmailAssigned()
        {
            this.entity.SetPhoneNumber(new PhoneNumber("+64277888111"));

            this.entity.Phone.Should().Be(new PhoneNumber("+64277888111"));
            this.entity.Events[1].Should().BeOfType<Events.Person.PhoneNumberChanged>();
        }
    }
}