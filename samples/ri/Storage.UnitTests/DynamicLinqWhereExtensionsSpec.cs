using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;

namespace Storage.UnitTests
{
    [TestClass]
    public class DynamicLinqWhereExtensionsSpec
    {
        [TestMethod, TestCategory("Unit")]
        public void WhenToDynamicLinqWhereClauseAndSingleCondition_ThenReturnsLinq()
        {
            var wheres = new List<WhereExpression>
            {
                new WhereExpression
                {
                    Condition = new WhereCondition
                    {
                        FieldName = "afield1",
                        Operator = ConditionOperator.EqualTo,
                        Value = "astringvalue"
                    }
                }
            };

            var result = wheres.ToDynamicLinqWhereClause();

            result.Should().Be("afield1 == \"astringvalue\"");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToDynamicLinqWhereClauseAndMultipleConditions_ThenReturnsLinq()
        {
            var wheres = new List<WhereExpression>
            {
                new WhereExpression
                {
                    Condition = new WhereCondition
                    {
                        FieldName = "afield1",
                        Operator = ConditionOperator.EqualTo,
                        Value = "astringvalue"
                    }
                },
                new WhereExpression
                {
                    Operator = LogicalOperator.And,
                    Condition = new WhereCondition
                    {
                        FieldName = "afield2",
                        Operator = ConditionOperator.GreaterThanEqualTo,
                        Value = "astringvalue"
                    }
                }
            };

            var result = wheres.ToDynamicLinqWhereClause();

            result.Should().Be("afield1 == \"astringvalue\" and afield2 >= \"astringvalue\"");
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToDynamicLinqWhereClauseAndNestedConditions_ThenReturnsLinq()
        {
            var wheres = new List<WhereExpression>
            {
                new WhereExpression
                {
                    Condition = new WhereCondition
                    {
                        FieldName = "afield1",
                        Operator = ConditionOperator.EqualTo,
                        Value = "astringvalue"
                    }
                },
                new WhereExpression
                {
                    Operator = LogicalOperator.And,
                    NestedWheres = new List<WhereExpression>
                    {
                        new WhereExpression
                        {
                            Condition = new WhereCondition
                            {
                                FieldName = "afield2",
                                Operator = ConditionOperator.EqualTo,
                                Value = "astringvalue2"
                            }
                        },
                        new WhereExpression
                        {
                            Operator = LogicalOperator.Or,
                            Condition = new WhereCondition
                            {
                                FieldName = "afield3",
                                Operator = ConditionOperator.EqualTo,
                                Value = "astringvalue3"
                            }
                        }
                    }
                }
            };

            var result = wheres.ToDynamicLinqWhereClause();

            result.Should()
                .Be("afield1 == \"astringvalue\" and (afield2 == \"astringvalue2\" or afield3 == \"astringvalue3\")");
        }
    }
}