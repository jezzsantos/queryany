using System;
using System.Collections.Generic;
using Application.Interfaces.Storage;
using FluentAssertions;
using QueryAny;
using Xunit;

namespace Application.Interfaces.UnitTests.Storage
{
    [Trait("Category", "Unit")]
    public class QueryAnyExtensionsSpec
    {
        [Fact]
        public void WhenWithSearchOptionsAndNullOptions_ThenThrows()
        {
            Query.Empty<TestEntity>()
                .Invoking(x => x.WithSearchOptions(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenWithSearchOptionsWithDefaultOptions_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions());

            query.ResultOptions.Offset.Should().Be(ResultOptions.DefaultOffset);
            query.ResultOptions.Limit.Should().Be(SearchOptions.DefaultLimit);
            query.ResultOptions.OrderBy.By.Should().BeNull();
            query.ResultOptions.OrderBy.Direction.Should().Be(OrderDirection.Ascending);
            query.PrimaryEntity.Selects.Count.Should().Be(0);
        }

        [Fact]
        public void WhenWithSearchOptionsWithOffset_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions {Offset = 9});

            query.ResultOptions.Offset.Should().Be(9);
        }

        [Fact]
        public void WhenWithSearchOptionsWithLimit_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions {Limit = 9});

            query.ResultOptions.Limit.Should().Be(9);
        }

        [Fact]
        public void WhenWithSearchOptionsWithUnknownSortProperty_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                    {Sort = new Sorting {By = "afieldname", Direction = SortDirection.Descending}});

            query.ResultOptions.OrderBy.By.Should().BeNull();
            query.ResultOptions.OrderBy.Direction.Should().Be(ResultOptions.DefaultOrderDirection);
        }

        [Fact]
        public void WhenWithSearchOptionsWithSortDescending_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                    {Sort = new Sorting {By = nameof(TestEntity.APropertyName), Direction = SortDirection.Descending}});

            query.ResultOptions.OrderBy.By.Should().Be(nameof(TestEntity.APropertyName));
            query.ResultOptions.OrderBy.Direction.Should().Be(OrderDirection.Descending);
        }

        [Fact]
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

        [Fact]
        public void WhenWithSearchOptionsWithLowerCaseFilterFields_ThenReturnsQuery()
        {
            var query = Query.Empty<TestEntity>()
                .WithSearchOptions(new SearchOptions
                {
                    Filter = new Filtering
                    {
                        Fields = new List<string>
                        {
                            nameof(TestEntity.APropertyName).ToLower()
                        }
                    }
                });

            query.PrimaryEntity.Selects.Count.Should().Be(1);
            query.PrimaryEntity.Selects[0].EntityName.Should().Be("Test");
            query.PrimaryEntity.Selects[0].FieldName.Should().Be(nameof(TestEntity.APropertyName));
            query.PrimaryEntity.Selects[0].JoinedEntityName.Should().BeNull();
            query.PrimaryEntity.Selects[0].JoinedFieldName.Should().BeNull();
        }

        [Fact]
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