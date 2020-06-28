using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;

namespace Storage.UnitTests
{
    [TestClass]
    public class AzureCosmosSqlApiWhereExtensionsSpec
    {
        private static readonly IAssertion Assert = new Assertion();

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndNoSelects_ThenReturnsSqlExpression()
        {
            var query = Query.Empty<TestEntity>();

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername");

            Assert.Equal("SELECT * FROM acontainername t", result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndSingleSelect_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue);

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername");

            Assert.Equal("SELECT t.Id, t.ABooleanValue FROM acontainername t", result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToAzureCosmosSqlApiWhereClauseAndMultipleSelects_ThenReturnsSqlExpression()
        {
            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.ABooleanValue)
                .Select(e => e.ADoubleValue);

            var result = query.ToAzureCosmosSqlApiWhereClause("acontainername");

            Assert.Equal("SELECT t.Id, t.ABooleanValue, t.ADoubleValue FROM acontainername t", result);
        }
    }
}