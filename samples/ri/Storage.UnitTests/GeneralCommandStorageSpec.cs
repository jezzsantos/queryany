using System;
using System.Collections.Generic;
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

            this.repository.Verify(repo => repo.Remove("acontainername", "anid".ToIdentifier()));
        }

        [TestMethod]
        public void WhenGet_ThenRetrievesFromRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid".ToIdentifier(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));

            this.commandStorage.Get("anid".ToIdentifier());

            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid".ToIdentifier(),
                    It.IsAny<RepositoryEntityMetadata>()));
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
            var addedEntity = new CommandEntity("anid");
            var fetchedEntity = new CommandEntity("anid");
            this.repository.Setup(repo => repo.Retrieve("acontainername", "anid".ToIdentifier(),
                    It.IsAny<RepositoryEntityMetadata>()))
                .Returns(fetchedEntity);
            this.repository.Setup(repo => repo.Add(It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns(addedEntity);

            this.commandStorage.Upsert(entity);

            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid".ToIdentifier(),
                    It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Add("acontainername", It.IsAny<CommandEntity>()));
        }

        [TestMethod]
        public void WhenUpsertAndEntityExists_ThenReplacesInRepository()
        {
            var entity = new TestEntity("anupsertedid".ToIdentifier()) {AStringValue = "anewvalue"};
            var fetchedEntity = new CommandEntity("anid");
            var updatedEntity = new CommandEntity("anid");
            var hydratedEntity = new TestEntity("anid".ToIdentifier());
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", It.IsAny<Identifier>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(fetchedEntity);
            this.repository.Setup(repo =>
                    repo.Replace("acontainername", It.IsAny<Identifier>(), It.IsAny<CommandEntity>()))
                .Returns(updatedEntity);
            this.domainFactory.Setup(df =>
                    df.RehydrateEntity(It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
                .Returns(hydratedEntity);

            var result = this.commandStorage.Upsert(entity);

            result.Should().BeEquivalentTo(hydratedEntity);
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anupsertedid".ToIdentifier(),
                    It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo =>
                repo.Replace("acontainername", "anupsertedid".ToIdentifier(), It.IsAny<CommandEntity>()));
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