using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using Storage.Azure;

namespace Storage.UnitTests
{
    [TestClass]
    public class AzureCosmosSqlApiWhereExtensionsSpec
    {
        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.Empty<TestEntity>();

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername");

            result.Should().Be("SELECT * FROM acontainername t");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndSingleSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue);

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername");

            result.Should().Be("SELECT t.id, t.ABooleanValue FROM acontainername t");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndMultipleSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue)
                .Select(e => e.ADoubleValue);

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername");

            result.Should().Be("SELECT t.id, t.ABooleanValue, t.ADoubleValue FROM acontainername t");
        }
    }
}