﻿using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Storage.UnitTests
{
    [Trait("Category", "Unit")]
    public class GeneralCrudStorageSpec
    {
        private readonly Mock<IRepository> repository;
        private readonly GeneralCrudStorage<TestDto> storage;

        public GeneralCrudStorageSpec()
        {
            var recorder = new Mock<IRecorder>();
            this.repository = new Mock<IRepository>();
            this.storage =
                new GeneralCrudStorage<TestDto>(recorder.Object, this.repository.Object);
        }

        [Fact]
        public void WhenDeleteAndNotExists_ThenReturns()
        {
            this.storage.Delete("anid".ToIdentifier());

            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Remove(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenDeleteAndDestroy_ThenRemovesFromRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));

            this.storage.Delete("anid".ToIdentifier());

            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Remove("acontainername", "anid"));
        }

        [Fact]
        public void WhenDeleteSoftAndAlreadySoftDeleted_ThenReturns()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid")
                {
                    IsDeleted = true
                });

            this.storage.Delete("anid".ToIdentifier(), false);

            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Remove(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenDeleteSoft_ThenReplacesToRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));

            this.storage.Delete("anid".ToIdentifier(), false);

            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Replace("acontainername", "anid", It.Is<CommandEntity>(ce =>
                ce.IsDeleted == true
            )));
        }

        [Fact]
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

        [Fact]
        public void WhenGetAndSoftDeleted_ThenReturnsNull()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid")
                {
                    IsDeleted = true
                });

            var result = this.storage.Get("anid".ToIdentifier());

            result.Should().BeNull();
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
        }

        [Fact]
        public void WhenGetAndSoftDeletedAndIncludeDeleted_ThenRetrievesFromRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid")
                {
                    IsDeleted = true
                });

            var result = this.storage.Get("anid".ToIdentifier(), true);

            result.Id.Should().Be("anid".ToIdentifier());
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
        }

        [Fact]
        public void WhenGet_ThenRetrievesFromRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));

            var result = this.storage.Get("anid".ToIdentifier());

            result.Id.Should().Be("anid".ToIdentifier());
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
        }

        [Fact]
        public void WhenResurrectAndEntityNotExists_ThenReturnsNull()
        {
            var result = this.storage.ResurrectDeleted("anid".ToIdentifier());

            result.Should().BeNull();
            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(
                repo => repo.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CommandEntity>()), Times.Never);
        }

        [Fact]
        public void WhenResurrectAndNotSoftDeleted_ThenReturnsEntity()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid"));

            var result = this.storage.ResurrectDeleted("anid".ToIdentifier());

            result.Should().NotBeNull();
            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(
                repo => repo.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CommandEntity>()), Times.Never);
        }

        [Fact]
        public void WhenResurrectAndSoftDeleted_ThenReturnsEntity()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid")
                {
                    IsDeleted = true
                });

            var result = this.storage.ResurrectDeleted("anid".ToIdentifier());

            result.Should().NotBeNull();
            this.repository.Verify(
                repo => repo.Retrieve("acontainername", "anid", It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Replace("acontainername", "anid", It.Is<CommandEntity>(ce =>
                ce.IsDeleted == false)));
        }

        [Fact]
        public void WhenUpsertAndEntityIdNotExists_ThenThrowsNotFound()
        {
            this.storage
                .Invoking(x => x.Upsert(new TestDto()))
                .Should().Throw<ResourceNotFoundException>();
        }

        [Fact]
        public void WhenUpsertAndEntityIdIsEmpty_ThenThrowsNotFound()
        {
            this.storage
                .Invoking(x => x.Upsert(new TestDto {Id = Identifier.Empty()}))
                .Should().Throw<ResourceNotFoundException>();
        }

        [Fact]
        public void WhenUpsertAndSoftDeleted_ThenThrowsNotFound()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid")
                {
                    IsDeleted = true
                });

            this.storage
                .Invoking(x => x.Upsert(new TestDto {Id = "anid".ToIdentifier()}))
                .Should().Throw<ResourceNotFoundException>();
        }

        [Fact]
        public void WhenUpsertAndSoftDeletedWithIncludeDeleted_ThenResurrectsAndReplacesInRepository()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new CommandEntity("anid")
                {
                    IsDeleted = true
                });
            this.repository.Setup(repo =>
                    repo.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns((string containerName, string id, CommandEntity entity) => entity);

            var result = this.storage.Upsert(new TestDto
            {
                Id = "anid".ToIdentifier(),
                AStringValue = "astringvalue",
                IsDeleted = true
            }, true);

            result.IsDeleted.Should().BeFalse();
            result.Id.Should().Be("anid".ToIdentifier());
        }

        [Fact]
        public void WhenUpsertAndEntityNotExists_ThenAddsToRepository()
        {
            var entity = new TestDto {Id = "anid".ToIdentifier()};
            var addedEntity = new CommandEntity("anid");
            this.repository.Setup(repo => repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()))
                .Returns((CommandEntity) null);
            this.repository.Setup(repo => repo.Add(It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns(addedEntity);

            this.storage.Upsert(entity);

            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo => repo.Add("acontainername", It.IsAny<CommandEntity>()));
        }

        [Fact]
        public void WhenUpsertAndEntityExists_ThenReplacesInRepository()
        {
            var entity = new TestDto {Id = "anupsertedid".ToIdentifier(), AStringValue = "anewvalue"};
            var fetchedEntity = new CommandEntity("anid");
            var updatedEntity = new CommandEntity("anid");
            var hydratedEntity = new TestDto {Id = "anid".ToIdentifier()};
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", It.IsAny<string>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(fetchedEntity);
            this.repository.Setup(repo =>
                    repo.Replace("acontainername", It.IsAny<string>(), It.IsAny<CommandEntity>()))
                .Returns(updatedEntity);

            var result = this.storage.Upsert(entity);

            result.Should().BeEquivalentTo(hydratedEntity);
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anupsertedid",
                    It.IsAny<RepositoryEntityMetadata>()));
            this.repository.Verify(repo =>
                repo.Replace("acontainername", "anupsertedid", It.IsAny<CommandEntity>()));
        }

        [Fact]
        public void WhenCount_ThenGetsCountFromRepo()
        {
            this.storage.Count();

            this.repository.Verify(repo => repo.Count("acontainername"));
        }

        [Fact]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.storage.DestroyAll();

            this.repository.Verify(repo => repo.DestroyAll("acontainername"));
        }
    }
}