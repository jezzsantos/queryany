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
    public class GenericQueryStoragePolicySpec
    {
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private GenericQueryStorage<TestEntity> queryStorage;
        private Mock<IRepository> repository;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.queryStorage =
                new TestQueryStorage(this.logger.Object, this.domainFactory.Object, this.repository.Object);
        }

        [TestMethod]
        public void WhenCount_ThenGetsCountFromRepo()
        {
            this.queryStorage.Count();

            this.repository.Verify(repo => repo.Count("acontainername"));
        }

        [TestMethod]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.queryStorage.DestroyAll();

            this.repository.Verify(repo => repo.DestroyAll("acontainername"));
        }

        [TestMethod]
        public void WhenQueryWithNullQuery_ThenReturnsEmptyResults()
        {
            var result = this.queryStorage.Query(null);

            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
            this.repository.Verify(
                repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<TestEntity>>(), It.IsAny<IDomainFactory>()),
                Times.Never);
        }

        [TestMethod]
        public void WhenQueryWithEmptyQuery_ThenReturnsEmptyResults()
        {
            var query = Query.Empty<TestEntity>();
            var result = this.queryStorage.Query(query);

            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
            this.repository.Verify(
                repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<TestEntity>>(), It.IsAny<IDomainFactory>()),
                Times.Never);
        }

        [TestMethod]
        public void WhenQuery_ThenQueriesRepo()
        {
            var query = Query.From<TestEntity>().WhereAll();
            var results = new List<TestEntity>();
            this.repository.Setup(repo => repo.Query("acontainername", query, this.domainFactory.Object))
                .Returns(results);

            var result = this.queryStorage.Query(query);

            result.Results.Should().BeEquivalentTo(results);
        }
    }
}