using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonsApplication.Storage;
using PersonsDomain;

namespace PersonsApplication.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class PersonsApplicationSpec
    {
        private Mock<ICurrentCaller> caller;
        private PersonsApplication carsApplication;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;
        private Mock<IPersonStorage> storage;
        private Mock<IEmailService> uniqueEmailService;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier());
            this.storage = new Mock<IPersonStorage>();
            this.uniqueEmailService = new Mock<IEmailService>();
            this.uniqueEmailService.Setup(ues => ues.EnsureEmailIsUnique(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            this.caller = new Mock<ICurrentCaller>();
            this.caller.Setup(c => c.Id).Returns("acallerid");
            this.carsApplication =
                new PersonsApplication(this.logger.Object, this.idFactory.Object, this.storage.Object,
                    this.uniqueEmailService.Object);
        }

        [TestMethod]
        public void WhenCreate_ThenReturnsPerson()
        {
            var entity = new PersonEntity(this.logger.Object, this.idFactory.Object, this.uniqueEmailService.Object);
            this.storage.Setup(s =>
                    s.Save(It.IsAny<PersonEntity>()))
                .Returns(entity);

            var result = this.carsApplication.Create(this.caller.Object, "afirstname", "alastname");

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
            this.storage.Setup(s => s.Load(It.IsAny<Identifier>()))
                .Returns(new PersonEntity(this.logger.Object, this.idFactory.Object, this.uniqueEmailService.Object));

            var result =
                this.carsApplication.Get(this.caller.Object, "anid", new GetOptions());

            result.Id.Should().Be("anid");
        }

        [TestMethod]
        public void WhenGetAnonymousUser_ThenReturnsAnonymousPerson()
        {
            var result =
                this.carsApplication.Get(this.caller.Object, CurrentCallerConstants.AnonymousUserId, new GetOptions());

            result.Id.Should().Be(CurrentCallerConstants.AnonymousUserId);
            result.Name.FirstName.Should().Be(CurrentCallerConstants.AnonymousUserName);
            result.Name.LastName.Should().Be(CurrentCallerConstants.AnonymousUserName);
        }
    }
}