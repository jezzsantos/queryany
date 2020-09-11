using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using Storage.Azure;

namespace Storage.UnitTests.Azure
{
    [TestClass, TestCategory("Unit")]

    // ReSharper disable once InconsistentNaming
    public class AzureStorageTableWhereExtensionsSpec
    {
        [TestMethod]
        public void WhenToAzureTableStorageWhereClauseAndSingleCondition_ThenReturnsSqlExpression()
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

            var result = wheres.ToAzureTableStorageWhereClause();

            result.Should().Be("afield1 eq 'astringvalue'");
        }

        [TestMethod]
        public void WhenToAzureTableStorageWhereClauseAndMultipleConditions_ThenReturnsSqlExpression()
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

            var result = wheres.ToAzureTableStorageWhereClause();

            result.Should().Be("afield1 eq 'astringvalue' and afield2 ge 'astringvalue'");
        }

        [TestMethod]
        public void WhenToAzureTableStorageWhereClauseAndNestedConditions_ThenReturnsSqlExpression()
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

            var result = wheres.ToAzureTableStorageWhereClause();

            result.Should()
                .Be("afield1 eq 'astringvalue' and (afield2 eq 'astringvalue2' or afield3 eq 'astringvalue3')");
        }
    }
}