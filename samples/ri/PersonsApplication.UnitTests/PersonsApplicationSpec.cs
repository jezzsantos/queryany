using Application.Interfaces;
using Common;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices.Interfaces;
using FluentAssertions;
using Moq;
using PersonsApplication.ReadModels;
using PersonsApplication.Storage;
using PersonsDomain;
using Xunit;

namespace PersonsApplication.UnitTests
{
    [Trait("Category", "Unit")]
    public class PersonsApplicationSpec
    {
        private readonly Mock<ICurrentCaller> caller;
        private readonly Mock<IIdentifierFactory> idFactory;
        private readonly PersonsApplication personsApplication;
        private readonly Mock<IRecorder> recorder;
        private readonly Mock<IPersonStorage> storage;
        private readonly Mock<IEmailService> uniqueEmailService;

        public PersonsApplicationSpec()
        {
            this.recorder = new Mock<IRecorder>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier());
            this.storage = new Mock<IPersonStorage>();
            this.uniqueEmailService = new Mock<IEmailService>();
            this.uniqueEmailService.Setup(ues => ues.EnsureEmailIsUnique(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            this.caller = new Mock<ICurrentCaller>();
            this.caller.Setup(c => c.Id).Returns("acallerid");
            this.personsApplication =
                new PersonsApplication(this.recorder.Object, this.idFactory.Object, this.storage.Object,
                    this.uniqueEmailService.Object);
        }

        [Fact]
        public void WhenCreate_ThenReturnsPerson()
        {
            var entity = new PersonEntity(this.recorder.Object, this.idFactory.Object, this.uniqueEmailService.Object);
            this.storage.Setup(s =>
                    s.Save(It.IsAny<PersonEntity>()))
                .Returns(entity);

            var result = this.personsApplication.Create(this.caller.Object, "afirstname", "alastname");

            result.Id.Should().Be("anid");
            this.storage.Verify(s =>
                s.Save(It.Is<PersonEntity>(e =>
                    e.Name == new PersonName("afirstname", "alastname")
                    && e.DisplayName == new PersonDisplayName("afirstname")
                )));
        }

        [Fact]
        public void WhenGet_ThenReturnsPerson()
        {
            this.storage.Setup(s => s.GetPerson(It.IsAny<Identifier>()))
                .Returns(new Person {Id = "anid"});

            var result =
                this.personsApplication.Get(this.caller.Object, "anid", new GetOptions());

            result.Id.Should().Be("anid");
        }

        [Fact]
        public void WhenGetAnonymousUser_ThenReturnsAnonymousPerson()
        {
            var result =
                this.personsApplication.Get(this.caller.Object, CallerConstants.AnonymousUserId,
                    new GetOptions());

            result.Id.Should().Be(CallerConstants.AnonymousUserId);
            result.Name.FirstName.Should().Be(CallerConstants.AnonymousUserName);
            result.Name.LastName.Should().Be(CallerConstants.AnonymousUserName);
        }
    }
}