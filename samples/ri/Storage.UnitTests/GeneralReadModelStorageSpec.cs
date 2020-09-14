using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GeneralReadModelStorageSpec
    {
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private GeneralReadModelStorage<TestReadModel> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.repository = new Mock<IRepository>();
            this.repository.Setup(repo => repo.Add(It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns((string containerName, CommandEntity entity) => new CommandEntity(entity.Id));
            this.storage =
                new GeneralReadModelStorage<TestReadModel>(this.logger.Object, this.repository.Object);
        }

        [TestMethod]
        public void WhenCreateWithNoAction_ThenCreatesAndReturnsDto()
        {
            var result = this.storage.Create("anid");

            result.Id.Should().Be("anid");
            this.repository.Verify(repo => repo.Add("acontainername", It.Is<CommandEntity>(entity =>
                entity.Id == "anid"
            )));
        }

        [TestMethod]
        public void WhenCreateWithInitialisingAction_ThenCreatesAndReturnsDto()
        {
            var result = this.storage.Create("anid", entity => entity.APropertyName = "avalue");

            result.Id.Should().Be("anid");
            this.repository.Verify(repo => repo.Add("acontainername", It.Is<CommandEntity>(entity =>
                entity.Id == "anid"
                && entity.Properties[nameof(TestReadModel.APropertyName)].ToString() == "avalue"
            )));
        }

        [TestMethod]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RepositoryEntityMetadata>()))
                .Returns((CommandEntity) null);

            this.storage
                .Invoking(x => x.Update("anid", entity => entity.APropertyName = "avalue"))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpdate_ThenUpdatesAndReturnsDto()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));
            this.repository.Setup(repo =>
                    repo.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns(new CommandEntity("anid"));

            var result = this.storage.Update("anid", entity => entity.APropertyName = "avalue");

            result.Id.Should().Be("anid");
            this.repository.Verify(repo => repo.Replace("acontainername", "anid", It.Is<CommandEntity>(
                entity =>
                    entity.Id == "anid"
                    && entity.Properties[nameof(TestReadModel.APropertyName)].ToString() == "avalue"
            )));
        }
    }
}