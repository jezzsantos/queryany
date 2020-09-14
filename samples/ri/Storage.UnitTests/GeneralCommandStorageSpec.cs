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
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private GeneralCommandStorage<TestDomainEntity> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.storage =
                new GeneralCommandStorage<TestDomainEntity>(this.logger.Object, this.domainFactory.Object,
                    this.repository.Object);
        }

        [TestMethod]
        public void WhenDelete_ThenRemovesFromRepository()
        {
            this.storage.Delete("anid".ToIdentifier());

            this.repository.Verify(repo => repo.Remove("acontainername", "anid"));
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns((CommandEntity) null);

            var result = this.storage.Get("anid".ToIdentifier());

            result.Should().BeNull();
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
        }

        [TestMethod]
        public void WhenGet_ThenRetrievesFromRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));

            this.storage.Get("anid".ToIdentifier());

            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdNotExists_ThenThrowsNotFound()
        {
            this.storage
                .Invoking(x => x.Upsert(new TestDomainEntity(null)))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdIsEmpty_ThenThrowsNotFound()
        {
            this.storage
                .Invoking(x => x.Upsert(new TestDomainEntity(Identifier.Empty())))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityNotExists_ThenAddsToRepository()
        {
            var entity = new TestDomainEntity("anid".ToIdentifier());
            var addedEntity = new CommandEntity("anid");
            var fetchedEntity = new CommandEntity("anid");
            this.repository.Setup(repo => repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()))
                .Returns(fetchedEntity);
            this.repository.Setup(repo => repo.Add(It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns(addedEntity);

            this.storage.Upsert(entity);

            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Add("acontainername", It.IsAny<CommandEntity>()));
        }

        [TestMethod]
        public void WhenUpsertAndEntityExists_ThenReplacesInRepository()
        {
            var entity = new TestDomainEntity("anupsertedid".ToIdentifier()) {AStringValue = "anewvalue"};
            var fetchedEntity = new CommandEntity("anid");
            var updatedEntity = new CommandEntity("anid");
            var hydratedEntity = new TestDomainEntity("anid".ToIdentifier());
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", It.IsAny<string>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(fetchedEntity);
            this.repository.Setup(repo =>
                    repo.Replace("acontainername", It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns(updatedEntity);
            this.domainFactory.Setup(df =>
                    df.RehydrateEntity(It.IsAny<Type>(), It.IsAny<IReadOnlyDictionary<string, object>>()))
                .Returns(hydratedEntity);

            var result = this.storage.Upsert(entity);

            result.Should().BeEquivalentTo(hydratedEntity);
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anupsertedid",
                    It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo =>
                repo.Replace("acontainername", "anupsertedid", It.IsAny<CommandEntity>()));
        }

        [TestMethod]
        public void WhenCount_ThenGetsCountFromRepo()
        {
            this.storage.Count();

            this.repository.Verify(repo => repo.Count("acontainername"));
        }

        [TestMethod]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.storage.DestroyAll();

            this.repository.Verify(repo => repo.DestroyAll("acontainername"));
        }
    }
}