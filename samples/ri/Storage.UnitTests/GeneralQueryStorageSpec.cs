using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GeneralQueryStorageSpec
    {
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private GeneralQueryStorage<TestDto> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.storage =
                new GeneralQueryStorage<TestDto>(this.logger.Object, this.domainFactory.Object,
                    this.repository.Object);
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void WhenQuery_ThenQueriesRepo()
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
    }
}