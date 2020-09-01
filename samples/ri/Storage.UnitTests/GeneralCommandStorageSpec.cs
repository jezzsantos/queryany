using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GeneralCommandStorageSpec
    {
        private GeneralCommandStorage<TestEntity> commandStorage;
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.commandStorage =
                new GeneralCommandStorage<TestEntity>(this.logger.Object, this.domainFactory.Object,
                    this.repository.Object);
        }

        [TestMethod]
        public void WhenDelete_ThenRemovesFromRepository()
        {
            this.commandStorage.Delete("anid".ToIdentifier());

            this.repository.Verify(repo => repo.Remove<TestEntity>("acontainername", "anid".ToIdentifier()));
        }

        [TestMethod]
        public void WhenGet_ThenRetrievesFromRepository()
        {
            this.commandStorage.Get("anid".ToIdentifier());

            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdNotExists_ThenThrowsNotFound()
        {
            this.commandStorage
                .Invoking(x => x.Upsert(new TestEntity(null)))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdIsEmpty_ThenThrowsNotFound()
        {
            this.commandStorage
                .Invoking(x => x.Upsert(new TestEntity(Identifier.Empty())))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityNotExists_ThenAddsToRepository()
        {
            var entity = new TestEntity("anid".ToIdentifier());
            this.repository.Setup(repo =>
                    repo.Add(It.IsAny<string>(), It.IsAny<TestEntity>(), It.IsAny<IDomainFactory>()))
                .Returns(entity);

            this.commandStorage.Upsert(entity);

            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
            this.repository.Verify(repo => repo.Add("acontainername", entity, this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenUpsertAndEntityExists_ThenReplacesInRepository()
        {
            var upsertedEntity = new TestEntity("anid".ToIdentifier()) {AStringValue = "anewvalue"};
            var fetchedEntity = new TestEntity("anid".ToIdentifier());
            var updatedEntity = new TestEntity("anid".ToIdentifier());
            this.repository.Setup(repo =>
                    repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object))
                .Returns(fetchedEntity);
            this.repository.Setup(repo =>
                    repo.Replace("acontainername", "anid".ToIdentifier(), fetchedEntity, this.domainFactory.Object))
                .Returns(updatedEntity);

            var result = this.commandStorage.Upsert(upsertedEntity);

            result.Should().Be(updatedEntity);
            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
            this.repository.Verify(repo =>
                repo.Replace("acontainername", "anid".ToIdentifier(), It.Is<TestEntity>(entity =>
                    entity.AStringValue == "anewvalue"
                ), this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenCount_ThenGetsCountFromRepo()
        {
            this.commandStorage.Count();

            this.repository.Verify(repo => repo.Count("acontainername"));
        }

        [TestMethod]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.commandStorage.DestroyAll();

            this.repository.Verify(repo => repo.DestroyAll("acontainername"));
        }
    }
}