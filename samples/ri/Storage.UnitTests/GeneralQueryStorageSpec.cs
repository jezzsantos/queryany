using System.Collections.Generic;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using QueryAny;
using Xunit;

namespace Storage.UnitTests
{
    [Trait("Category", "Unit")]
    public class GeneralQueryStorageSpec
    {
        private readonly Mock<IRepository> repository;
        private readonly GeneralQueryStorage<TestDto> storage;

        public GeneralQueryStorageSpec()
        {
            var recorder = new Mock<IRecorder>();
            var domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.storage =
                new GeneralQueryStorage<TestDto>(recorder.Object, domainFactory.Object,
                    this.repository.Object);
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

        [Fact]
        public void WhenQueryWithNullQuery_ThenReturnsEmptyResults()
        {
            var result = this.storage.Query(null);

            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
            this.repository.Verify(
                repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<IQueryableEntity>>(),
                    It.IsAny<RepositoryEntityMetadata>()),
                Times.Never);
        }

        [Fact]
        public void WhenQueryWithEmptyQuery_ThenReturnsEmptyResults()
        {
            var query = Query.Empty<TestDto>();
            var result = this.storage.Query(query);

            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
            this.repository.Verify(
                repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<TestDto>>(),
                    It.IsAny<RepositoryEntityMetadata>()),
                Times.Never);
        }

        [Fact]
        public void WhenQueryAndDeleted_ThenReturnsNonDeletedResults()
        {
            var query = Query.From<TestDto>().WhereAll();
            var results = new List<QueryEntity>
            {
                new QueryEntity {Id = "anid1", IsDeleted = true},
                new QueryEntity {Id = "anid2", IsDeleted = false},
                new QueryEntity {Id = "anid3", IsDeleted = null}
            };
            this.repository.Setup(repo =>
                    repo.Query("acontainername", It.IsAny<QueryClause<TestDto>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(results);

            var result = this.storage.Query(query);

            result.Results.Count.Should().Be(2);
            result.Results[0].Id.Should().Be("anid2");
            result.Results[1].Id.Should().Be("anid3");
        }

        [Fact]
        public void WhenQueryAndDeletedAndIncludeDeleted_ThenReturnsDeletedResults()
        {
            var query = Query.From<TestDto>().WhereAll();
            var results = new List<QueryEntity>
            {
                new QueryEntity {Id = "anid1", IsDeleted = true},
                new QueryEntity {Id = "anid2", IsDeleted = false},
                new QueryEntity {Id = "anid3", IsDeleted = null}
            };
            this.repository.Setup(repo =>
                    repo.Query("acontainername", It.IsAny<QueryClause<TestDto>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(results);

            var result = this.storage.Query(query, true);

            result.Results.Count.Should().Be(3);
            result.Results[0].Id.Should().Be("anid1");
            result.Results[1].Id.Should().Be("anid2");
            result.Results[2].Id.Should().Be("anid3");
        }

        [Fact]
        public void WhenQuery_ThenReturnsAllResults()
        {
            var query = Query.From<TestDto>().WhereAll();
            var results = new List<QueryEntity>();
            this.repository.Setup(repo =>
                    repo.Query("acontainername", It.IsAny<QueryClause<TestDto>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(results);

            var result = this.storage.Query(query);

            result.Results.Should().BeEquivalentTo(results);
        }

        [Fact]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            this.repository.Setup(repo =>
                    repo.Retrieve("acontainername", "anid",
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns((CommandEntity) null);

            var result = this.storage.Get<TestDto>("anid".ToIdentifier());

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

            var result = this.storage.Get<TestDto>("anid".ToIdentifier());

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

            var result = this.storage.Get<TestDto>("anid".ToIdentifier(), true);

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

            var result = this.storage.Get<TestDto>("anid".ToIdentifier());

            result.Id.Should().Be("anid".ToIdentifier());
            this.repository.Verify(repo =>
                repo.Retrieve("acontainername", "anid",
                    It.IsAny<RepositoryEntityMetadata>()));
        }
    }
}