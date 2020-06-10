using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace QueryAny.UnitTests
{
    public class QuerySpec
    {
        private static readonly IAssertion Assert = new Assertion();

        [TestClass]
        public class GivenAContext
        {
            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithNullColumn_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Query.Create(null, QueryOperator.EQ, "avalue"));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithStringValue_ThenReturnsSingleExpression()
            {
                var result = Query.Create("acolumnname", QueryOperator.EQ, "avalue");

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithDateTimeValue_ThenReturnsSingleExpression()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create("acolumnname", QueryOperator.EQ, now);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(now, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithBoolValue_ThenReturnsSingleExpression()
            {
                var result = Query.Create("acolumnname", QueryOperator.EQ, true);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(true, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithStringProperty_ThenReturnsSingleExpression()
            {
                var result = Query.Create<TestTableEntity>(x => x.Field1, QueryOperator.EQ, "avalue");

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("Field1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithDateTimeProperty_ThenReturnsSingleExpression()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create<TestTableEntity>(x => x.DateTime1, QueryOperator.EQ, now);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(now, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateWithBoolProperty_ThenReturnsSingleExpression()
            {
                var result = Query.Create<TestTableEntity>(x => x.Bool1, QueryOperator.EQ, true);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("Bool1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(true, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateEmptyDateWithTimeProperty_ThenReturnsSingleExpression()
            {
                var result = Query.CreateEmptyDate<TestTableEntity>(x => x.DateTime1);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(DateTime.MinValue, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenCreateNotEmptyDateWithTimeProperty_ThenReturnsSingleExpression()
            {
                var result = Query.CreateNotEmptyDate<TestTableEntity>(x => x.DateTime1);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions.First().Condition.Operator);
                Assert.Equal(DateTime.MinValue, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithNullColumn_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Query.Empty().From(null, QueryOperator.EQ, "avalue"));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithStringValue_ThenReturnsQueryWithExpression()
            {
                var result = Query.Empty().From("acolumnname", QueryOperator.EQ, "avalue");

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithDateTimeValue_ThenReturnsQueryWithExpression()
            {
                var now = DateTime.UtcNow;
                var result = Query.Empty().From("acolumnname", QueryOperator.EQ, now);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(now, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithBoolValue_ThenReturnsQueryWithExpression()
            {
                var result = Query.Empty().From("acolumnname", QueryOperator.EQ, true);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(true, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithStringProperty_ThenReturnsQueryWithExpression()
            {
                var result = Query.Empty().From<TestTableEntity>(x => x.Field1, QueryOperator.EQ, "avalue");

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("Field1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithDateTimeProperty_ThenReturnsQueryWithExpression()
            {
                var now = DateTime.UtcNow;
                var result = Query.Empty().From<TestTableEntity>(x => x.DateTime1, QueryOperator.EQ, now);

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal("DateTime1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal(now, result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithString_ThenReturnsQueryWithAndedExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .From("acolumnname2", QueryOperator.NE, "avalue2");

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenFromWithDateTime_ThenReturnsQueryWithAndedExpressions()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .From("acolumnname2", QueryOperator.NE, now);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(now, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithNullColumn_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Query.Create("acolumnname1", QueryOperator.EQ, "avalue1").And(null, QueryOperator.EQ, "avalue"));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithNoPreviousExpressions_ThenThrows()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Query.Empty().And("acolumnname", QueryOperator.EQ, "avalue"));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithStringValue_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And("acolumnname2", QueryOperator.NE, "avalue2");

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithDateTimeValue_ThenReturnsQueryWithExpressions()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And("acolumnname2", QueryOperator.NE, now);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(now, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithBoolValue_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And("acolumnname2", QueryOperator.NE, true);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(true, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithStringProperty_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And<TestTableEntity>(x => x.Field1, QueryOperator.NE, "avalue2");

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("Field1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithDateTimeProperty_ThenReturnsQueryWithExpressions()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And<TestTableEntity>(x => x.DateTime1, QueryOperator.NE, now);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(now, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithBoolProperty_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And<TestTableEntity>(x => x.Bool1, QueryOperator.NE, true);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("Bool1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(true, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithNullQuery_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Query.Empty().And((Query) null));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithNoExpressions_ThenDoesNotAddExpressions()
            {
                var result = Query.Empty()
                    .And(Query.Empty());

                Assert.False(result.Expressions.Any());
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndAndEmptyWithQueryWithExpression_ThenCreatesExpression()
            {
                var result = Query.Empty()
                    .And(Query.Create("acolumnname", QueryOperator.EQ, "avalue"));

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue", result.Expressions.First().Condition.Value);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndWithQueryWithExpression_ThenAndsSubExpression()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And(Query.Create("acolumnname2", QueryOperator.EQ, "avalue2"));

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

                var addedExpression = result.Expressions[1];
                Assert.Equal(CombineOperator.AND, addedExpression.Combiner);
                Assert.Null(addedExpression.Condition);
                Assert.Equal(1, addedExpression.NestedExpressions.Count);
                Assert.Equal("acolumnname2", addedExpression.NestedExpressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, addedExpression.NestedExpressions.First().Condition.Operator);
                Assert.Equal("avalue2", addedExpression.NestedExpressions.First().Condition.Value);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndEmptyDateWithQueryWithExpression_ThenAndsSubExpression()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .AndEmptyDate<TestTableEntity>(x => x.DateTime1);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

                var addedExpression = result.Expressions[1];
                Assert.Equal(CombineOperator.AND, addedExpression.Combiner);
                Assert.Equal("DateTime1", addedExpression.Condition.Column);
                Assert.Equal(QueryOperator.EQ, addedExpression.Condition.Operator);
                Assert.Equal(DateTime.MinValue, addedExpression.Condition.Value);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndNotEmptyDateWithQueryWithExpression_ThenAndsSubExpression()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .AndNotEmptyDate<TestTableEntity>(x => x.DateTime1);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

                var addedExpression = result.Expressions[1];
                Assert.Equal(CombineOperator.AND, addedExpression.Combiner);
                Assert.Equal("DateTime1", addedExpression.Condition.Column);
                Assert.Equal(QueryOperator.NE, addedExpression.Condition.Operator);
                Assert.Equal(DateTime.MinValue, addedExpression.Condition.Value);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithNullColumn_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Query.Create("acolumnname1", QueryOperator.EQ, "avalue1").Or(null, QueryOperator.EQ, "avalue"));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithNoPreviousExpressions_ThenThrows()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Query.Empty().Or("acolumnname", QueryOperator.EQ, "avalue"));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithStringValue_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or("acolumnname2", QueryOperator.NE, "avalue2");

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithDateTimeValue_ThenReturnsQueryWithExpressions()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or("acolumnname2", QueryOperator.NE, now);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(now, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithBoolValue_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or("acolumnname2", QueryOperator.NE, true);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("acolumnname2", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(true, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithStringProperty_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or<TestTableEntity>(x => x.Field1, QueryOperator.NE, "avalue2");

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("Field1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal("avalue2", result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithDateTimeProperty_ThenReturnsQueryWithExpressions()
            {
                var now = DateTime.UtcNow;
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or<TestTableEntity>(x => x.DateTime1, QueryOperator.NE, now);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(now, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithBoolProperty_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or<TestTableEntity>(x => x.Bool1, QueryOperator.NE, true);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("Bool1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(true, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrEmptyDateWithDateTimeProperty_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .OrEmptyDate<TestTableEntity>(x => x.DateTime1);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions[1].Condition.Operator);
                Assert.Equal(DateTime.MinValue, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrNotEmptyDateWithDateTimeProperty_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .OrNotEmptyDate<TestTableEntity>(x => x.DateTime1);

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);
                Assert.Equal("DateTime1", result.Expressions[1].Condition.Column);
                Assert.Equal(QueryOperator.NE, result.Expressions[1].Condition.Operator);
                Assert.Equal(DateTime.MinValue, result.Expressions[1].Condition.Value);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Null(result.Expressions[1].NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndSubqueryWithNoPreviousExpressions_ThenThrows()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Query.Empty().And(sq => sq.From("acolumnname", QueryOperator.EQ, "avalue")));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenAndSubquery_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And(sq => sq.From("acolumnname2", QueryOperator.NE, "avalue2"));

                Assert.Equal(2, result.Expressions.Count);

                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);

                Assert.Null(result.Expressions[1].Condition);
                Assert.Equal(CombineOperator.AND, result.Expressions[1].Combiner);
                Assert.Equal(1, result.Expressions[1].NestedExpressions.Count);

                var subExpression = result.Expressions[1].NestedExpressions.First();
                Assert.Equal("acolumnname2", subExpression.Condition.Column);
                Assert.Equal(QueryOperator.NE, subExpression.Condition.Operator);
                Assert.Equal("avalue2", subExpression.Condition.Value);
                Assert.Equal(CombineOperator.None, subExpression.Combiner);
                Assert.Null(subExpression.NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrSubqueryWithNoPreviousExpressions_ThenThrows()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Query.Empty().Or(sq => sq.From("acolumnname", QueryOperator.EQ, "avalue")));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrSubquery_ThenReturnsQueryWithExpressions()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or(sq => sq.From("acolumnname2", QueryOperator.NE, "avalue2"));

                Assert.Equal(2, result.Expressions.Count);

                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Null(result.Expressions.First().NestedExpressions);

                Assert.Null(result.Expressions[1].Condition);
                Assert.Equal(CombineOperator.OR, result.Expressions[1].Combiner);
                Assert.Equal(1, result.Expressions[1].NestedExpressions.Count);

                var subExpression = result.Expressions[1].NestedExpressions.First();
                Assert.Equal("acolumnname2", subExpression.Condition.Column);
                Assert.Equal(QueryOperator.NE, subExpression.Condition.Operator);
                Assert.Equal("avalue2", subExpression.Condition.Value);
                Assert.Equal(CombineOperator.None, subExpression.Combiner);
                Assert.Null(subExpression.NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithNullQuery_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Query.Empty().Or((Query) null));
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithNoExpressions_ThenDoesNotAddExpressions()
            {
                var result = Query.Empty()
                    .Or(Query.Empty());

                Assert.False(result.Expressions.Any());
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrAndEmptyWithQueryWithExpression_ThenCreatesExpression()
            {
                var result = Query.Empty()
                    .Or(Query.Create("acolumnname", QueryOperator.EQ, "avalue"));

                Assert.Equal(1, result.Expressions.Count);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Equal("acolumnname", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue", result.Expressions.First().Condition.Value);
                Assert.Null(result.Expressions.First().NestedExpressions);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenOrWithQueryWithExpression_ThenOrsSubExpression()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .Or(Query.Create("acolumnname2", QueryOperator.EQ, "avalue2"));

                Assert.Equal(2, result.Expressions.Count);
                Assert.Equal(CombineOperator.None, result.Expressions.First().Combiner);
                Assert.Equal("acolumnname1", result.Expressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, result.Expressions.First().Condition.Operator);
                Assert.Equal("avalue1", result.Expressions.First().Condition.Value);

                var addedExpression = result.Expressions[1];
                Assert.Equal(CombineOperator.OR, addedExpression.Combiner);
                Assert.Null(addedExpression.Condition);
                Assert.Equal(1, addedExpression.NestedExpressions.Count);
                Assert.Equal("acolumnname2", addedExpression.NestedExpressions.First().Condition.Column);
                Assert.Equal(QueryOperator.EQ, addedExpression.NestedExpressions.First().Condition.Operator);
                Assert.Equal("avalue2", addedExpression.NestedExpressions.First().Condition.Value);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndNoExpressions_ThenReturnsEmptyString()
            {
                var result = Query.Empty().ToQueryString();

                Assert.Equal(string.Empty, result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndSingleStringNullValue_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname", QueryOperator.EQ, null).ToQueryString();

                Assert.Equal(
                    AzureCosmosTableQuery.GenerateFilterCondition("acolumnname",
                        QueryOperator.EQ.ToString().ToLowerInvariant(), null), result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndSingleStringValue_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname", QueryOperator.EQ, "avalue").ToQueryString();

                Assert.Equal(
                    AzureCosmosTableQuery.GenerateFilterCondition("acolumnname",
                        QueryOperator.EQ.ToString().ToLowerInvariant(), "avalue"), result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndSingleDateTimeNullValue_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname", QueryOperator.EQ, DateTime.MinValue).ToQueryString();

                Assert.Equal(
                    AzureCosmosTableQuery.GenerateFilterConditionForDate("acolumnname",
                        QueryOperator.EQ.ToString().ToLowerInvariant(),
                        new DateTimeOffset(AzureCosmosTableQuery.MinAzureDateTime)), result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndSingleDateTimeValue_ThenReturnsQueryString()
            {
                var date = DateTime.UtcNow;
                var result = Query.Create("acolumnname", QueryOperator.EQ, date).ToQueryString();

                Assert.Equal(
                    AzureCosmosTableQuery.GenerateFilterConditionForDate("acolumnname",
                        QueryOperator.EQ.ToString().ToLowerInvariant(), new DateTimeOffset(date)), result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndSingleBoolValue_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname", QueryOperator.EQ, true).ToQueryString();

                Assert.Equal(
                    AzureCosmosTableQuery.GenerateFilterCondition("acolumnname",
                        QueryOperator.EQ.ToString().ToLowerInvariant(), true.ToLower()), result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndAndedExpressions_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname1", QueryOperator.GT, "avalue1")
                    .And("acolumnname2", QueryOperator.LT, "avalue2")
                    .ToQueryString();

                var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                    QueryOperator.GT.ToString().ToLowerInvariant(), "avalue1");
                var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                    QueryOperator.LT.ToString().ToLowerInvariant(), "avalue2");
                var query = AzureCosmosTableQuery.CombineFilters(query1,
                    CombineOperator.AND.ToString().ToLowerInvariant(), query2);

                Assert.Equal(query, result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndOredExpressions_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname1", QueryOperator.GT, "avalue1")
                    .Or("acolumnname2", QueryOperator.LT, "avalue2")
                    .ToQueryString();

                var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                    QueryOperator.GT.ToString().ToLowerInvariant(), "avalue1");
                var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                    QueryOperator.LT.ToString().ToLowerInvariant(), "avalue2");
                var query = AzureCosmosTableQuery.CombineFilters(query1,
                    CombineOperator.OR.ToString().ToLowerInvariant(), query2);

                Assert.Equal(query, result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndMixedExpressions_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname1", QueryOperator.GT, "avalue1")
                    .Or("acolumnname2", QueryOperator.LT, "avalue2")
                    .And("acolumnname3", QueryOperator.EQ, "avalue3")
                    .ToQueryString();

                var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                    QueryOperator.GT.ToString().ToLowerInvariant(), "avalue1");
                var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                    QueryOperator.LT.ToString().ToLowerInvariant(), "avalue2");
                var query3 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname3",
                    QueryOperator.EQ.ToString().ToLowerInvariant(), "avalue3");
                var query = AzureCosmosTableQuery.CombineFilters(
                    AzureCosmosTableQuery.CombineFilters(query1, CombineOperator.OR.ToString().ToLowerInvariant(),
                        query2),
                    CombineOperator.AND.ToString().ToLowerInvariant(), query3);

                Assert.Equal(query, result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndNestedExpressions_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And(sub => sub.From("acolumnname2", QueryOperator.GT, "avalue2")
                        .Or("acolumnname3", QueryOperator.LT, "avalue3"))
                    .ToQueryString();

                var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                    QueryOperator.EQ.ToString().ToLowerInvariant(), "avalue1");
                var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                    QueryOperator.GT.ToString().ToLowerInvariant(), "avalue2");
                var query3 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname3",
                    QueryOperator.LT.ToString().ToLowerInvariant(), "avalue3");
                var query = AzureCosmosTableQuery.CombineFilters(query1,
                    CombineOperator.AND.ToString().ToLowerInvariant(),
                    AzureCosmosTableQuery.CombineFilters(query2, CombineOperator.OR.ToString().ToLowerInvariant(),
                        query3));

                Assert.Equal(query, result);
            }

            [TestMethod, TestCategory("Unit")]
            public void WhenToQueryStringAndNestedQuery_ThenReturnsQueryString()
            {
                var result = Query.Create("acolumnname1", QueryOperator.EQ, "avalue1")
                    .And(Query.Create("acolumnname2", QueryOperator.GT, "avalue2")
                        .Or("acolumnname3", QueryOperator.LT, "avalue3"))
                    .ToQueryString();

                var query1 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname1",
                    QueryOperator.EQ.ToString().ToLowerInvariant(), "avalue1");
                var query2 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname2",
                    QueryOperator.GT.ToString().ToLowerInvariant(), "avalue2");
                var query3 = AzureCosmosTableQuery.GenerateFilterCondition("acolumnname3",
                    QueryOperator.LT.ToString().ToLowerInvariant(), "avalue3");
                var query = AzureCosmosTableQuery.CombineFilters(query1,
                    CombineOperator.AND.ToString().ToLowerInvariant(),
                    AzureCosmosTableQuery.CombineFilters(query2, CombineOperator.OR.ToString().ToLowerInvariant(),
                        query3));

                Assert.Equal(query, result);
            }
        }
    }
}