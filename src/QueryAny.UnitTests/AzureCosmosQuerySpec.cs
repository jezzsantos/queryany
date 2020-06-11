using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace QueryAny.UnitTests
{
    [TestClass]
    public class AzureCosmosQuerySpec
    {
        private static readonly IAssertion Assert = new Assertion();

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithNullColumn_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() => AzureCosmosQuery.Create(null, Condition.Eq, "avalue"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithStringValue_ThenReturnsSingleExpression()
        {
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, "avalue");

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithDateTimeValue_ThenReturnsSingleExpression()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, now);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(now, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithBoolValue_ThenReturnsSingleExpression()
        {
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, true);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(true, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithStringProperty_ThenReturnsSingleExpression()
        {
            var result = AzureCosmosQuery.Create<TestTableEntity>(x => x.Field1, Condition.Eq, "avalue");

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("Field1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithDateTimeProperty_ThenReturnsSingleExpression()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create<TestTableEntity>(x => x.DateTime1, Condition.Eq, now);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(now, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateWithBoolProperty_ThenReturnsSingleExpression()
        {
            var result = AzureCosmosQuery.Create<TestTableEntity>(x => x.Bool1, Condition.Eq, true);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("Bool1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(true, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateEmptyDateWithTimeProperty_ThenReturnsSingleExpression()
        {
            var result = AzureCosmosQuery.CreateEmptyDate<TestTableEntity>(x => x.DateTime1);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(DateTime.MinValue, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCreateNotEmptyDateWithTimeProperty_ThenReturnsSingleExpression()
        {
            var result = AzureCosmosQuery.CreateNotEmptyDate<TestTableEntity>(x => x.DateTime1);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions.First().Condition.Operator);
            Assert.Equal(DateTime.MinValue, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithNullColumn_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() => AzureCosmosQuery.Empty().From(null, Condition.Eq, "avalue"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithStringValue_ThenReturnsQueryWithExpression()
        {
            var result = AzureCosmosQuery.Empty().From("acolumnname", Condition.Eq, "avalue");

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithDateTimeValue_ThenReturnsQueryWithExpression()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Empty().From("acolumnname", Condition.Eq, now);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(now, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithBoolValue_ThenReturnsQueryWithExpression()
        {
            var result = AzureCosmosQuery.Empty().From("acolumnname", Condition.Eq, true);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(true, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithStringProperty_ThenReturnsQueryWithExpression()
        {
            var result = AzureCosmosQuery.Empty().From<TestTableEntity>(x => x.Field1, Condition.Eq, "avalue");

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("Field1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithDateTimeProperty_ThenReturnsQueryWithExpression()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Empty().From<TestTableEntity>(x => x.DateTime1, Condition.Eq, now);

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal(now, result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithString_ThenReturnsQueryWithAndedExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .From("acolumnname2", Condition.Ne, "avalue2");

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFromWithDateTime_ThenReturnsQueryWithAndedExpressions()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .From("acolumnname2", Condition.Ne, now);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(now, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithNullColumn_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                    .And(null, Condition.Eq, "avalue"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithNoPreviousExpressions_ThenThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AzureCosmosQuery.Empty().And("acolumnname", Condition.Eq, "avalue"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithStringValue_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And("acolumnname2", Condition.Ne, "avalue2");

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithDateTimeValue_ThenReturnsQueryWithExpressions()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And("acolumnname2", Condition.Ne, now);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(now, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithBoolValue_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And("acolumnname2", Condition.Ne, true);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(true, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithStringProperty_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And<TestTableEntity>(x => x.Field1, Condition.Ne, "avalue2");

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("Field1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithDateTimeProperty_ThenReturnsQueryWithExpressions()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And<TestTableEntity>(x => x.DateTime1, Condition.Ne, now);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(now, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithBoolProperty_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And<TestTableEntity>(x => x.Bool1, Condition.Ne, true);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("Bool1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(true, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithNullQuery_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AzureCosmosQuery.Empty().And((AzureCosmosQuery) null));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithNoExpressions_ThenDoesNotAddExpressions()
        {
            var result = AzureCosmosQuery.Empty()
                .And(AzureCosmosQuery.Empty());

            Assert.False(result.Expressions.Any());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndAndEmptyWithQueryWithExpression_ThenCreatesExpression()
        {
            var result = AzureCosmosQuery.Empty()
                .And(AzureCosmosQuery.Create("acolumnname", Condition.Eq, "avalue"));

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue", result.Expressions.First().Condition.Value);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndWithQueryWithExpression_ThenAndsSubExpression()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And(AzureCosmosQuery.Create("acolumnname2", Condition.Eq, "avalue2"));

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

            var addedExpression = result.Expressions[1];
            Assert.Equal(Combine.And, addedExpression.Combiner);
            Assert.Null(addedExpression.Condition);
            Assert.Equal(1, addedExpression.NestedExpressions.Count);
            Assert.Equal("acolumnname2", addedExpression.NestedExpressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, addedExpression.NestedExpressions.First().Condition.Operator);
            Assert.Equal("avalue2", addedExpression.NestedExpressions.First().Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndEmptyDateWithQueryWithExpression_ThenAndsSubExpression()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .AndEmptyDate<TestTableEntity>(x => x.DateTime1);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

            var addedExpression = result.Expressions[1];
            Assert.Equal(Combine.And, addedExpression.Combiner);
            Assert.Equal("DateTime1", addedExpression.Condition.Column);
            Assert.Equal(Condition.Eq, addedExpression.Condition.Operator);
            Assert.Equal(DateTime.MinValue, addedExpression.Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndNotEmptyDateWithQueryWithExpression_ThenAndsSubExpression()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .AndNotEmptyDate<TestTableEntity>(x => x.DateTime1);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

            var addedExpression = result.Expressions[1];
            Assert.Equal(Combine.And, addedExpression.Combiner);
            Assert.Equal("DateTime1", addedExpression.Condition.Column);
            Assert.Equal(Condition.Ne, addedExpression.Condition.Operator);
            Assert.Equal(DateTime.MinValue, addedExpression.Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithNullColumn_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                    .Or(null, Condition.Eq, "avalue"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithNoPreviousExpressions_ThenThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AzureCosmosQuery.Empty().Or("acolumnname", Condition.Eq, "avalue"));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithStringValue_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or("acolumnname2", Condition.Ne, "avalue2");

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithDateTimeValue_ThenReturnsQueryWithExpressions()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or("acolumnname2", Condition.Ne, now);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(now, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithBoolValue_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or("acolumnname2", Condition.Ne, true);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(true, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithStringProperty_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or<TestTableEntity>(x => x.Field1, Condition.Ne, "avalue2");

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("Field1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithDateTimeProperty_ThenReturnsQueryWithExpressions()
        {
            var now = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or<TestTableEntity>(x => x.DateTime1, Condition.Ne, now);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(now, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithBoolProperty_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or<TestTableEntity>(x => x.Bool1, Condition.Ne, true);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("Bool1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(true, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrEmptyDateWithDateTimeProperty_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .OrEmptyDate<TestTableEntity>(x => x.DateTime1);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions[1].Condition.Operator);
            Assert.Equal(DateTime.MinValue, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrNotEmptyDateWithDateTimeProperty_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .OrNotEmptyDate<TestTableEntity>(x => x.DateTime1);

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);
            Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
            Assert.Equal(Condition.Ne, result.Expressions[1].Condition.Operator);
            Assert.Equal(DateTime.MinValue, result.Expressions[1].Condition.Value);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Null(result.Expressions[1].NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndSubqueryWithNoPreviousExpressions_ThenThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AzureCosmosQuery.Empty().And(sq => sq.From("acolumnname", Condition.Eq, "avalue")));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAndSubquery_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And(sq => sq.From("acolumnname2", Condition.Ne, "avalue2"));

            Assert.Equal(2, result.Expressions.Count);

            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);

            Assert.Null(result.Expressions[1].Condition);
            Assert.Equal(Combine.And, result.Expressions[1].Combiner);
            Assert.Equal(1, result.Expressions[1].NestedExpressions.Count);

            var subExpression = result.Expressions[1].NestedExpressions.First();
            Assert.Equal("acolumnname2", subExpression.Condition.Column);
            Assert.Equal(Condition.Ne, subExpression.Condition.Operator);
            Assert.Equal("avalue2", subExpression.Condition.Value);
            Assert.Equal(Combine.None, subExpression.Combiner);
            Assert.Null(subExpression.NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrSubqueryWithNoPreviousExpressions_ThenThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                AzureCosmosQuery.Empty().Or(sq => sq.From("acolumnname", Condition.Eq, "avalue")));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrSubquery_ThenReturnsQueryWithExpressions()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or(sq => sq.From("acolumnname2", Condition.Ne, "avalue2"));

            Assert.Equal(2, result.Expressions.Count);

            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Null(result.Expressions.First().NestedExpressions);

            Assert.Null(result.Expressions[1].Condition);
            Assert.Equal(Combine.Or, result.Expressions[1].Combiner);
            Assert.Equal(1, result.Expressions[1].NestedExpressions.Count);

            var subExpression = result.Expressions[1].NestedExpressions.First();
            Assert.Equal("acolumnname2", subExpression.Condition.Column);
            Assert.Equal(Condition.Ne, subExpression.Condition.Operator);
            Assert.Equal("avalue2", subExpression.Condition.Value);
            Assert.Equal(Combine.None, subExpression.Combiner);
            Assert.Null(subExpression.NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithNullQuery_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AzureCosmosQuery.Empty().Or((AzureCosmosQuery) null));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithNoExpressions_ThenDoesNotAddExpressions()
        {
            var result = AzureCosmosQuery.Empty()
                .Or(AzureCosmosQuery.Empty());

            Assert.False(result.Expressions.Any());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrAndEmptyWithQueryWithExpression_ThenCreatesExpression()
        {
            var result = AzureCosmosQuery.Empty()
                .Or(AzureCosmosQuery.Create("acolumnname", Condition.Eq, "avalue"));

            Assert.Equal(1, result.Expressions.Count);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue", result.Expressions.First().Condition.Value);
            Assert.Null(result.Expressions.First().NestedExpressions);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOrWithQueryWithExpression_ThenOrsSubExpression()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .Or(AzureCosmosQuery.Create("acolumnname2", Condition.Eq, "avalue2"));

            Assert.Equal(2, result.Expressions.Count);
            Assert.Equal(Combine.None, result.Expressions.First().Combiner);
            Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, result.Expressions.First().Condition.Operator);
            Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

            var addedExpression = result.Expressions[1];
            Assert.Equal(Combine.Or, addedExpression.Combiner);
            Assert.Null(addedExpression.Condition);
            Assert.Equal(1, addedExpression.NestedExpressions.Count);
            Assert.Equal("acolumnname2", addedExpression.NestedExpressions.First().Condition.Column);
            Assert.Equal(Condition.Eq, addedExpression.NestedExpressions.First().Condition.Operator);
            Assert.Equal("avalue2", addedExpression.NestedExpressions.First().Condition.Value);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndNoExpressions_ThenReturnsEmptyString()
        {
            var result = AzureCosmosQuery.Empty().ToQueryString();

            Assert.Equal(string.Empty, result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndSingleStringNullValue_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, null).ToQueryString();

            Assert.Equal(
                AzureCosmosTableQuery.GenerateFilterCondition("acolumnname",
                    Condition.Eq.ToString().ToLowerInvariant(), null), result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndSingleStringValue_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, "avalue").ToQueryString();

            Assert.Equal(
                AzureCosmosTableQuery.GenerateFilterCondition("acolumnname",
                    Condition.Eq.ToString().ToLowerInvariant(), "avalue"), result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndSingleDateTimeNullValue_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, DateTime.MinValue).ToQueryString();

            Assert.Equal(
                AzureCosmosTableQuery.GenerateFilterConditionForDate("acolumnname",
                    Condition.Eq.ToString().ToLowerInvariant(),
                    new DateTimeOffset(AzureCosmosTableQuery.MinAzureDateTime)), result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndSingleDateTimeValue_ThenReturnsQueryString()
        {
            var date = DateTime.UtcNow;
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, date).ToQueryString();

            Assert.Equal(
                AzureCosmosTableQuery.GenerateFilterConditionForDate("acolumnname",
                    Condition.Eq.ToString().ToLowerInvariant(), new DateTimeOffset(date)), result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndSingleBoolValue_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname", Condition.Eq, true).ToQueryString();

            Assert.Equal(
                AzureCosmosTableQuery.GenerateFilterCondition("acolumnname",
                    Condition.Eq.ToString().ToLowerInvariant(), true.ToLower()), result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndAndedExpressions_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Gt, "avalue1")
                .And("acolumnname2", Condition.Lt, "avalue2")
                .ToQueryString();

            var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                Condition.Gt.ToString().ToLowerInvariant(), "avalue1");
            var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                Condition.Lt.ToString().ToLowerInvariant(), "avalue2");
            var query = AzureCosmosTableQuery.CombineFilters(query1,
                Combine.And.ToString().ToLowerInvariant(), query2);

            Assert.Equal(query, result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndOredExpressions_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Gt, "avalue1")
                .Or("acolumnname2", Condition.Lt, "avalue2")
                .ToQueryString();

            var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                Condition.Gt.ToString().ToLowerInvariant(), "avalue1");
            var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                Condition.Lt.ToString().ToLowerInvariant(), "avalue2");
            var query = AzureCosmosTableQuery.CombineFilters(query1,
                Combine.Or.ToString().ToLowerInvariant(), query2);

            Assert.Equal(query, result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndMixedExpressions_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Gt, "avalue1")
                .Or("acolumnname2", Condition.Lt, "avalue2")
                .And("acolumnname3", Condition.Eq, "avalue3")
                .ToQueryString();

            var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                Condition.Gt.ToString().ToLowerInvariant(), "avalue1");
            var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                Condition.Lt.ToString().ToLowerInvariant(), "avalue2");
            var query3 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname3",
                Condition.Eq.ToString().ToLowerInvariant(), "avalue3");
            var query = AzureCosmosTableQuery.CombineFilters(
                AzureCosmosTableQuery.CombineFilters(query1, Combine.Or.ToString().ToLowerInvariant(),
                    query2),
                Combine.And.ToString().ToLowerInvariant(), query3);

            Assert.Equal(query, result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndNestedExpressions_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And(sub => sub.From("acolumnname2", Condition.Gt, "avalue2")
                    .Or("acolumnname3", Condition.Lt, "avalue3"))
                .ToQueryString();

            var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                Condition.Eq.ToString().ToLowerInvariant(), "avalue1");
            var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                Condition.Gt.ToString().ToLowerInvariant(), "avalue2");
            var query3 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname3",
                Condition.Lt.ToString().ToLowerInvariant(), "avalue3");
            var query = AzureCosmosTableQuery.CombineFilters(query1,
                Combine.And.ToString().ToLowerInvariant(),
                AzureCosmosTableQuery.CombineFilters(query2, Combine.Or.ToString().ToLowerInvariant(),
                    query3));

            Assert.Equal(query, result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToQueryStringAndNestedQuery_ThenReturnsQueryString()
        {
            var result = AzureCosmosQuery.Create("acolumnname1", Condition.Eq, "avalue1")
                .And(AzureCosmosQuery.Create("acolumnname2", Condition.Gt, "avalue2")
                    .Or("acolumnname3", Condition.Lt, "avalue3"))
                .ToQueryString();

            var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                Condition.Eq.ToString().ToLowerInvariant(), "avalue1");
            var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                Condition.Gt.ToString().ToLowerInvariant(), "avalue2");
            var query3 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname3",
                Condition.Lt.ToString().ToLowerInvariant(), "avalue3");
            var query = AzureCosmosTableQuery.CombineFilters(query1,
                Combine.And.ToString().ToLowerInvariant(),
                AzureCosmosTableQuery.CombineFilters(query2, Combine.Or.ToString().ToLowerInvariant(),
                    query3));

            Assert.Equal(query, result);
        }
    }
}