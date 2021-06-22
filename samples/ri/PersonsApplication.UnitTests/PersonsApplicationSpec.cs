using Application.Interfaces;
using Common;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonsApplication.ReadModels;
using PersonsApplication.Storage;
using PersonsDomain;

namespace PersonsApplication.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class PersonsApplicationSpec
    {
        private Mock<ICurrentCaller> caller;
        private Mock<IIdentifierFactory> idFactory;
        private PersonsApplication personsApplication;
        private Mock<IRecorder> recorder;
        private Mock<IPersonStorage> storage;
        private Mock<IEmailService> uniqueEmailService;

        [TestInitialize]
        public void Initialize()
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

        [TestMethod]
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

        [TestMethod]
        public void WhenGet_ThenReturnsPerson()
        {
            this.storage.Setup(s => s.GetPerson(It.IsAny<Identifier>()))
                .Returns(new Person {Id = "anid"});

            var result =
                this.personsApplication.Get(this.caller.Object, "anid", new GetOptions());

            result.Id.Should().Be("anid");
        }

        [TestMethod]
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