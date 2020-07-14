using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Services.Interfaces.UnitTests
{
    [TestClass]
    public class SearchOptionsSpec
    {
        private SearchOptionsDto hasSearchOptions;

        [TestInitialize]
        public void Initialize()
        {
            this.hasSearchOptions = new SearchOptionsDto();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndNullOptions_ThenReturnsNull()
        {
            var result = ((SearchOptionsDto) null).ToSearchOptions();

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndAllUndefined_ThenReturnsSearchOptions()
        {
            var result = this.hasSearchOptions.ToSearchOptions();

            result.Limit.Should().Be(SearchOptions.DefaultLimit);
            result.Offset.Should().Be(SearchOptions.NoOffset);
            Assert.IsNull(result.Sort.By);
            result.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Filter.Fields.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndLimit_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = 9;
            this.hasSearchOptions.Offset = 99;

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Limit.Should().Be(9);
            result.Offset.Should().Be(99);
            Assert.IsNull(result.Sort.By);
            result.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Filter.Fields.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndNoLimit_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = SearchOptions.NoLimit;
            this.hasSearchOptions.Offset = 99;

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Limit.Should().Be(SearchOptions.DefaultLimit);
            result.Offset.Should().Be(99);
            Assert.IsNull(result.Sort.By);
            result.Sort.Direction.Should().Be(SortDirection.Ascending);
            result.Filter.Fields.Count.Should().Be(0);
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndFilters_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Filter = "Field1;Field2";

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Filter.Fields.Count.Should().Be(2);
            result.Filter.Fields[0].Should().Be("Field1");
            result.Filter.Fields[1].Should().Be("Field2");
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndDistinct_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Distinct = "adistinct";

            var result = this.hasSearchOptions.ToSearchOptions();

            result.Distinct.Should().Be("adistinct");
        }
    }

    public class SearchOptionsDto : IHasSearchOptions
    {
        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }

        public string Distinct { get; set; }

        public string Embed { get; set; }
    }
}