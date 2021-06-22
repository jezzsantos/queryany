using Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace Storage.UnitTests
{
    [Trait("Category", "Unit")]
    public class GeneralReadModelStorageSpec
    {
        private readonly Mock<IRepository> repository;
        private readonly GeneralReadModelStorage<TestReadModel> storage;

        public GeneralReadModelStorageSpec()
        {
            var recorder = new Mock<IRecorder>();
            this.repository = new Mock<IRepository>();
            this.repository.Setup(repo => repo.Add(It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns((string containerName, CommandEntity entity) => new CommandEntity(entity.Id));
            this.storage =
                new GeneralReadModelStorage<TestReadModel>(recorder.Object, this.repository.Object);
        }

        [Fact]
        public void WhenCreateWithNoAction_ThenCreatesAndReturnsDto()
        {
            var result = this.storage.Create("anid");

            result.Id.Should().Be("anid");
            this.repository.Verify(repo => repo.Add("acontainername", It.Is<CommandEntity>(entity =>
                entity.Id == "anid"
            )));
        }

        [Fact]
        public void WhenCreateWithInitialisingAction_ThenCreatesAndReturnsDto()
        {
            var result = this.storage.Create("anid", entity => entity.APropertyName = "avalue");

            result.Id.Should().Be("anid");
            this.repository.Verify(repo => repo.Add("acontainername", It.Is<CommandEntity>(entity =>
                entity.Id == "anid"
                && entity.Properties[nameof(TestReadModel.APropertyName)].ToString() == "avalue"
            )));
        }

        [Fact]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RepositoryEntityMetadata>()))
                .Returns((CommandEntity) null);

            this.storage
                .Invoking(x => x.Update("anid", entity => entity.APropertyName = "avalue"))
                .Should().Throw<ResourceNotFoundException>();
        }

        [Fact]
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