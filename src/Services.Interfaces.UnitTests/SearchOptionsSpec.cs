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

            Assert.AreEqual(SearchOptions.DefaultLimit, result.Limit);
            Assert.AreEqual(SearchOptions.NoOffset, result.Offset);
            Assert.IsNull(result.Sort.By);
            Assert.AreEqual(SortDirection.Ascending, result.Sort.Direction);
            Assert.AreEqual(0, result.Filter.Fields.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndLimit_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = 9;
            this.hasSearchOptions.Offset = 99;

            var result = this.hasSearchOptions.ToSearchOptions();

            Assert.AreEqual(9, result.Limit);
            Assert.AreEqual(99, result.Offset);
            Assert.IsNull(result.Sort.By);
            Assert.AreEqual(SortDirection.Ascending, result.Sort.Direction);
            Assert.AreEqual(0, result.Filter.Fields.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndNoLimit_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = SearchOptions.NoLimit;
            this.hasSearchOptions.Offset = 99;

            var result = this.hasSearchOptions.ToSearchOptions();

            Assert.AreEqual(SearchOptions.DefaultLimit, result.Limit);
            Assert.AreEqual(99, result.Offset);
            Assert.IsNull(result.Sort.By);
            Assert.AreEqual(SortDirection.Ascending, result.Sort.Direction);
            Assert.AreEqual(0, result.Filter.Fields.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndSingleSort_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Sort = "+Field1";

            var result = this.hasSearchOptions.ToSearchOptions();

            Assert.AreEqual("Field1", result.Sort.By);
            Assert.AreEqual(SortDirection.Ascending, result.Sort.Direction);

            this.hasSearchOptions.Sort = "-Field1";

            result = this.hasSearchOptions.ToSearchOptions();

            Assert.AreEqual("Field1", result.Sort.By);
            Assert.AreEqual(SortDirection.Descending, result.Sort.Direction);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndFilters_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Filter = "Field1;Field2";

            var result = this.hasSearchOptions.ToSearchOptions();

            Assert.AreEqual(2, result.Filter.Fields.Count);
            Assert.AreEqual("Field1", result.Filter.Fields[0]);
            Assert.AreEqual("Field2", result.Filter.Fields[1]);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndAllUndefinedWithDefaults_ThenReturnsSearchOptions()
        {
            var result = this.hasSearchOptions.ToSearchOptions(9, 99, "-asort", "afilter");

            Assert.AreEqual(9, result.Limit);
            Assert.AreEqual(99, result.Offset);
            Assert.AreEqual("asort", result.Sort.By);
            Assert.AreEqual(SortDirection.Descending, result.Sort.Direction);
            Assert.AreEqual(1, result.Filter.Fields.Count);
            Assert.AreEqual("afilter", result.Filter.Fields[0]);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndAllUndefinedWithDefaultMaxLimit_ThenReturnsSearchOptions()
        {
            var result = this.hasSearchOptions.ToSearchOptions(0, 99, "-asort", "afilter");

            Assert.AreEqual(SearchOptions.DefaultLimit, result.Limit);
            Assert.AreEqual(99, result.Offset);
            Assert.AreEqual("asort", result.Sort.By);
            Assert.AreEqual(SortDirection.Descending, result.Sort.Direction);
            Assert.AreEqual(1, result.Filter.Fields.Count);
            Assert.AreEqual("afilter", result.Filter.Fields[0]);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsWithDefaults_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Limit = 6;
            this.hasSearchOptions.Offset = 66;
            this.hasSearchOptions.Sort = "-asort1";
            this.hasSearchOptions.Filter = "afilter1";

            var result = this.hasSearchOptions.ToSearchOptions(9, 99, "asort2", "afilter2");

            Assert.AreEqual(6, result.Limit);
            Assert.AreEqual(66, result.Offset);
            Assert.AreEqual("asort1", result.Sort.By);
            Assert.AreEqual(SortDirection.Descending, result.Sort.Direction);
            Assert.AreEqual(1, result.Filter.Fields.Count);
            Assert.AreEqual("afilter1", result.Filter.Fields[0]);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToSearchOptionsAndDistinct_ThenReturnsSearchOptions()
        {
            this.hasSearchOptions.Distinct = "adistinct";

            var result = this.hasSearchOptions.ToSearchOptions();

            Assert.AreEqual("adistinct", result.Distinct);
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