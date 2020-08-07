using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Api.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class HasSearchOptionsSpec
    {
        private SearchOptionsDto hasSearchOptions;

        [TestInitialize]
        public void Initialize()
        {
            this.hasSearchOptions = new SearchOptionsDto();
        }

        [TestMethod]
        public void WhenToSearchOptionsAndNullOptions_ThenReturnsNull()
        {
            var result = ((SearchOptionsDto) null).ToSearchOptions();

            result.Should().BeNull();
        }

        [TestMethod]
        public void WhenToSearchOptionsAndAllUndefined_ThenReturnsSearchOptions()
        {
            var result = this.hasSearchOptions.ToSearchOptions();

            result.Limit.Should().Be(SearchOptions.DefaultLimit);
            result.Offset.Should().Be(SearchOptions.NoOffset);
            result.Sort.By.Should().BeNull();
            result.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Filter.Fields.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenToSearchOptionsAndLimit_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = 9;
            this.hasSearchOptions.Offset = 99;

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Limit.Should().Be(9);
            result.Offset.Should().Be(99);
            result.Sort.By.Should().BeNull();
            result.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Filter.Fields.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenToSearchOptionsAndNoLimit_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = SearchOptions.NoLimit;
            this.hasSearchOptions.Offset = 99;

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Limit.Should().Be(SearchOptions.DefaultLimit);
            result.Offset.Should().Be(99);
            result.Sort.By.Should().BeNull();
            result.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Filter.Fields.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenToSearchOptionsAndSingleSort_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Sort = "+Field1";

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Sort.By.Should().Be("Field1");
            result.Sort.Direction.Should().Be(SortDirection.Ascending);

            this.hasSearchOptions.Sort = "-Field1";

            result = this.hasSearchOptions.ToSearchOptions();

            result.Sort.By.Should().Be("Field1");
            result.Sort.Direction.Should().Be(SortDirection.Descending);
        }

        [TestMethod]
        public void WhenToSearchOptionsAndFilters_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Filter = "Field1;Field2";

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Filter.Fields.Count.Should().Be(2);
            result.Filter.Fields[0].Should().Be("Field1");
            result.Filter.Fields[1].Should().Be("Field2");
        }

        [TestMethod]
        public void WhenToSearchOptionsAndAllUndefinedWithDefaults_ThenReturnsSearchOptions()
        {
            var result = this.hasSearchOptions.ToSearchOptions(9, 99, "-asort", "afilter");

            result.Limit.Should().Be(9);
            result.Offset.Should().Be(99);
            result.Sort.By.Should().Be("asort");
            result.Sort.Direction.Should().Be(SortDirection.Descending);
            result.Filter.Fields.Count.Should().Be(1);
            result.Filter.Fields[0].Should().Be("afilter");
        }

        [TestMethod]
        public void WhenToSearchOptionsAndAllUndefinedWithDefaultMaxLimit_ThenReturnsSearchOptions()
        {
            var result = this.hasSearchOptions.ToSearchOptions(0, 99, "-asort", "afilter");

            result.Limit.Should().Be(SearchOptions.DefaultLimit);
            result.Offset.Should().Be(99);
            result.Sort.By.Should().Be("asort");
            result.Sort.Direction.Should().Be(SortDirection.Descending);
            result.Filter.Fields.Count.Should().Be(1);
            result.Filter.Fields[0].Should().Be("afilter");
        }

        [TestMethod]
        public void WhenToSearchOptionsWithDefaults_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = 6;
            this.hasSearchOptions.Offset = 66;
            this.hasSearchOptions.Sort = "-asort1";
            this.hasSearchOptions.Filter = "afilter1";

            var result = this.hasSearchOptions.ToSearchOptions(9, 99, "asort2", "afilter2");

            result.Limit.Should().Be(6);
            result.Offset.Should().Be(66);
            result.Sort.By.Should().Be("asort1");
            result.Sort.Direction.Should().Be(SortDirection.Descending);
            result.Filter.Fields.Count.Should().Be(1);
            result.Filter.Fields[0].Should().Be("afilter1");
        }

        [TestMethod]
        public void WhenToMetadataSafeWithNullSearchOptions_ThenReturnsDefaultSearchMetadata()
        {
            var results = ((SearchOptions) null).ToMetadataSafe();

            results.Total.Should().Be(0);
        }

        [TestMethod]
        public void WhenToMetadataSafeWithNullSearchOptionsAndTotal_ThenReturnsDefaultSearchMetadata()
        {
            var results = ((SearchOptions) null).ToMetadataSafe(11);

            results.Total.Should().Be(11);
        }

        [TestMethod]
        public void WhenToMetadataSafeWithInitialSearchOptions_ThenReturnsSearchMetadata()
        {
            var searchOptions = new SearchOptions();

            var results = searchOptions.ToMetadataSafe();

            results.Total.Should().Be(0);
        }

        [TestMethod]
        public void WhenToMetadataSafe_ThenReturnsPopulatedSearchMetadata()
        {
            var searchOptions = new SearchOptions
            {
                Sort = new Sorting
                {
                    Direction = SortDirection.Descending,
                    By = "asortfield"
                },
                Offset = 9,
                Limit = 6,
                Filter = new Filtering
                {
                    Fields = new List<string> {"afilterfield"}
                }
            };

            var results = searchOptions.ToMetadataSafe();

            results.Total.Should().Be(0);
            results.Sort.Direction.Should().Be(SortDirection.Descending);
            results.Sort.By.Should().Be("asortfield");
            results.Offset.Should().Be(9);
            results.Limit.Should().Be(6);
            results.Filter.Fields.Count.Should().Be(1);
            results.Filter.Fields[0].Should().Be("afilterfield");
        }

        [TestMethod]
        public void WhenToMetadataSafeAndTotal_ThenReturnsPopulatedSearchMetadata()
        {
            var searchOptions = new SearchOptions
            {
                Sort = new Sorting
                {
                    Direction = SortDirection.Descending,
                    By = "asortfield"
                },
                Offset = 9,
                Limit = 6,
                Filter = new Filtering
                {
                    Fields = new List<string> {"afilterfield"}
                }
            };

            var results = searchOptions.ToMetadataSafe(11);

            results.Total.Should().Be(11);
            results.Sort.Direction.Should().Be(SortDirection.Descending);
            results.Sort.By.Should().Be("asortfield");
            results.Offset.Should().Be(9);
            results.Limit.Should().Be(6);
            results.Filter.Fields.Count.Should().Be(1);
            results.Filter.Fields[0].Should().Be("afilterfield");
        }
    }

    [TestClass, TestCategory("Unit")]
    public class GivenASearchOptions
    {
        private SearchOptions searchOptions;

        [TestInitialize]
        public void Initialize()
        {
            this.searchOptions = new SearchOptions();
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndNoLimit_ThenTakesDefaultLimit()
        {
            this.searchOptions.Limit = SearchOptions.NoLimit;
            var queried = Enumerable.Range(1, SearchOptions.MaxLimit + 1).ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            result.Results.Count().Should().Be(SearchOptions.DefaultLimit);
            result.Metadata.Filter.Fields.Any().Should().BeFalse();
            result.Metadata.Sort.By.Should().BeNull();
            result.Metadata.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Metadata.Offset.Should().Be(SearchOptions.NoOffset);
            result.Metadata.Limit.Should().Be(SearchOptions.NoLimit);
            result.Metadata.Total.Should().Be(SearchOptions.MaxLimit + 1);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndLimitLessThanMax_ThenTakesLimit()
        {
            this.searchOptions.Limit = SearchOptions.MaxLimit - 1;
            var queried = Enumerable.Range(1, SearchOptions.MaxLimit + 1).ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            result.Results.Count().Should().Be(SearchOptions.MaxLimit - 1);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(SearchOptions.MaxLimit + 1);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndLimitGreaterThanMax_ThenTakesMaxLimit()
        {
            this.searchOptions.Limit = SearchOptions.MaxLimit + 1;
            var queried = Enumerable.Range(1, SearchOptions.MaxLimit + 1).ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            result.Results.Count().Should().Be(SearchOptions.MaxLimit);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(SearchOptions.MaxLimit + 1);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndLimitLessThanMaxAndQueriedLessThanLimit_ThenTakesMaxQueried()
        {
            this.searchOptions.Limit = SearchOptions.MaxLimit - 1;
            var queried = Enumerable.Range(1, 66).ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            result.Results.Count().Should().Be(66);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(66);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndLimitGreaterThanMaxAndQueriedLessThanLimit_ThenTakesMaxQueried()
        {
            this.searchOptions.Limit = SearchOptions.MaxLimit + 1;
            var queried = Enumerable.Range(1, 66).ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            result.Results.Count().Should().Be(66);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(66);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndSortIsNull_ThenNoOrdering()
        {
            this.searchOptions.Sort = null;
            var queried = new[]
            {
                1,
                6,
                3
            }.ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            var results = result.Results.ToList();
            results.Count.Should().Be(3);
            results[0].Should().Be(1);
            results[1].Should().Be(6);
            results[2].Should().Be(3);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(3);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndSortByIsEmpty_ThenNoOrdering()
        {
            this.searchOptions.Sort = new Sorting {By = string.Empty};
            var queried = new[]
            {
                1,
                6,
                3
            }.ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            var results = result.Results.ToList();
            results.Count.Should().Be(3);
            results[0].Should().Be(1);
            results[1].Should().Be(6);
            results[2].Should().Be(3);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(3);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndSortByIsUnknown_ThenNoOrdering()
        {
            this.searchOptions.Sort = new Sorting {By = "unknown"};
            var queried = new[]
            {
                1,
                6,
                3
            }.ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            var results = result.Results.ToList();
            results.Count.Should().Be(3);
            results[0].Should().Be(1);
            results[1].Should().Be(6);
            results[2].Should().Be(3);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(3);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndSortDirectionDescending_ThenOrderingByDefault()
        {
            this.searchOptions.Sort = new Sorting {By = "AProperty"};
            var queried = new[]
            {
                new Sortable {AProperty = 1},
                new Sortable {AProperty = 6},
                new Sortable {AProperty = 3}
            }.ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            var results = result.Results.ToList();
            results.Count.Should().Be(3);
            results[0].AProperty.Should().Be(1);
            results[1].AProperty.Should().Be(3);
            results[2].AProperty.Should().Be(6);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(3);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndSortDirectionAscending_ThenOrderingAscending()
        {
            this.searchOptions.Sort = new Sorting {By = "AProperty", Direction = SortDirection.Ascending};
            var queried = new[]
            {
                new Sortable {AProperty = 1},
                new Sortable {AProperty = 6},
                new Sortable {AProperty = 3}
            }.ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            var results = result.Results.ToList();
            results.Count.Should().Be(3);
            results[0].AProperty.Should().Be(1);
            results[1].AProperty.Should().Be(3);
            results[2].AProperty.Should().Be(6);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(3);
        }

        [TestMethod]
        public void WhenApplyWithMetadataAndSortDirectionDescending_ThenOrderingDescending()
        {
            this.searchOptions.Sort = new Sorting {By = "AProperty", Direction = SortDirection.Descending};
            var queried = new[]
            {
                new Sortable {AProperty = 1},
                new Sortable {AProperty = 6},
                new Sortable {AProperty = 3}
            }.ToList();

            var result = this.searchOptions.ApplyWithMetadata(queried);

            var results = result.Results.ToList();
            results.Count.Should().Be(3);
            results[0].AProperty.Should().Be(6);
            results[1].AProperty.Should().Be(3);
            results[2].AProperty.Should().Be(1);
            AssertSearchResults(result);
            result.Metadata.Total.Should().Be(3);
        }

        private void AssertSearchResults<T>(SearchResults<T> result)
        {
            result.Metadata.Filter.Fields.Should().BeSameAs(this.searchOptions.Filter.Fields);
            if (this.searchOptions.Sort != null)
            {
                result.Metadata.Sort.By.Should().Be(this.searchOptions.Sort.By);
                result.Metadata.Sort.Direction.Should().Be(this.searchOptions.Sort.Direction);
            }
            else
            {
                result.Metadata.Sort.Should().BeNull();
            }

            result.Metadata.Offset.Should().Be(this.searchOptions.Offset);
            result.Metadata.Limit.Should().Be(this.searchOptions.Limit);
        }
    }

    public class SearchOptionsDto : IHasSearchOptions
    {
        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }

        public string Embed { get; set; }
    }

    public class Sortable
    {
        public int AProperty { get; set; }

        public string AnotherProperty { get; set; }
    }
}