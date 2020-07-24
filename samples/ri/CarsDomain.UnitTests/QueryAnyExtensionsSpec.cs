using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using Services.Interfaces;

namespace CarsDomain.UnitTests
{
    [TestClass]
    public class QueryAnyExtensionsSpec
    {
        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsAndNullOptions_ThenThrows()
        {
            Query.Empty<TestEntity>()
                .Invoking(x => x.WithSearchOptions(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithDefaultOptions_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions());

            query.ResultOptions.Offset.Should().Be(ResultOptions.DefaultOffset);
            query.ResultOptions.Limit.Should().Be(SearchOptions.DefaultLimit);
            query.ResultOptions.Order.By.Should().BeNull();
            query.ResultOptions.Order.Direction.Should().Be(OrderDirection.Ascending);
            query.PrimaryEntity.Selects.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithOffset_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions {Offset = 9});

            query.ResultOptions.Offset.Should().Be(9);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithLimit_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions {Limit = 9});

            query.ResultOptions.Limit.Should().Be(9);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithUnknownSortProperty_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                    {Sort = new Sorting {By = "afieldname", Direction = SortDirection.Descending}});

            query.ResultOptions.Order.By.Should().BeNull();
            query.ResultOptions.Order.Direction.Should().Be(ResultOptions.DefaultOrderDirection);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithSortDescending_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                    {Sort = new Sorting {By = nameof(TestEntity.APropertyName), Direction = SortDirection.Descending}});

            query.ResultOptions.Order.By.Should().Be(nameof(TestEntity.APropertyName));
            query.ResultOptions.Order.Direction.Should().Be(OrderDirection.Descending);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithUnknownFilterFields_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                {
                    Filter = new Filtering
                    {
                        Fields = new List<string>
                        {
                            "afieldname"
                        }
                    }
                });

            query.PrimaryEntity.Selects.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenWithSearchOptionsWithFilterFields_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                {
                    Filter = new Filtering
                    {
                        Fields = new List<string>
                        {
                            nameof(TestEntity.APropertyName)
                        }
                    }
                });

            query.PrimaryEntity.Selects.Count.Should().Be(1);
            query.PrimaryEntity.Selects[0].EntityName.Should().Be("Test");
            query.PrimaryEntity.Selects[0].FieldName.Should().Be(nameof(TestEntity.APropertyName));
            query.PrimaryEntity.Selects[0].JoinedEntityName.Should().BeNull();
            query.PrimaryEntity.Selects[0].JoinedFieldName.Should().BeNull();
        }
    }
}