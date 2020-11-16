using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class RepositoryExtensionsSpec
    {
        [TestMethod]
        public void
            WhenGetDefaultOrderingAndNoOrderingAndEntityNotHaveDefaultOrderingPropertyNorBackupPropertyName_ThenReturnsFirstPropertyNameOrdering()
        {
            var result = Query.From<TestDtoWithoutDefaultOrderingOrBackupProperty>().WhereAll()
                .GetDefaultOrdering();

            result.Should().Be(nameof(TestDtoWithoutDefaultOrderingOrBackupProperty.APropertyName));
        }

        [TestMethod]
        public void
            WhenGetDefaultOrderingAndNoOrderingAndEntityNotHaveDefaultOrderingProperty_ThenReturnsBackupOrdering()
        {
            var result = Query.From<TestDtoWithoutDefaultOrderingProperty>().WhereAll()
                .GetDefaultOrdering();

            result.Should().Be(RepositoryExtensions.BackupOrderingPropertyName);
        }

        [TestMethod]
        public void WhenGetDefaultOrderingAndNoOrdering_ThenReturnsDefaultOrdering()
        {
            var result = Query.From<TestDtoWithDefaultProperty>().WhereAll()
                .GetDefaultOrdering();

            result.Should().Be(RepositoryExtensions.DefaultOrderingPropertyName);
        }

        // [TestMethod]
        // public void WhenGetDefaultOrderingAndOrderingNotExistsOnEntity_ThenReturnsDefaultOrdering()
        // {
        //     var result = Query.From<TestDtoWithDefaultProperty>().WhereAll()
        //         .OrderBy(x=> x.Unknown)
        //         .GetDefaultOrdering();
        //
        //     result.Should().Be(RepositoryExtensions.DefaultOrderingPropertyName);
        // }

        [TestMethod]
        public void WhenGetDefaultOrderingAndOrderingExistsOnEntity_ThenReturnsNamedOrdering()
        {
            var result = Query.From<TestDtoWithDefaultProperty>().WhereAll()
                .OrderBy(x => x.APropertyName)
                .GetDefaultOrdering();

            result.Should().Be(nameof(TestDtoWithDefaultProperty.APropertyName));
        }

        [TestMethod]
        public void WhenGetDefaultOrderingWithSelectionsAndNoOrdering_ThenReturnsDefaultOrdering()
        {
            var result = Query.From<TestDtoWithDefaultProperty>()
                .WhereAll()
                .Select(x => x.LastPersistedAtUtc)
                .GetDefaultOrdering();

            result.Should().Be(RepositoryExtensions.DefaultOrderingPropertyName);
        }

        [TestMethod]
        public void WhenGetDefaultOrderingWithSelectionsAndNoOrderingAndNoDefaultSelection_ThenReturnsBackupOrdering()
        {
            var result = Query.From<TestDtoWithDefaultProperty>()
                .WhereAll()
                .Select(x => x.Id)
                .GetDefaultOrdering();

            result.Should().Be(RepositoryExtensions.BackupOrderingPropertyName);
        }

        [TestMethod]
        public void
            WhenGetDefaultOrderingWithSelectionsAndNoOrderingAndNoBackupSelection_ThenReturnsFirstSelectedPropertyOrdering()
        {
            var result = Query.From<TestDtoWithDefaultProperty>()
                .WhereAll()
                .Select(x => x.APropertyName)
                .GetDefaultOrdering();

            result.Should().Be(nameof(TestDtoWithDefaultProperty.APropertyName));
        }

        [TestMethod]
        public void WhenGetDefaultOrderingWithSelectionsAndOrdering_ThenReturnsSelectedNamedOrdering()
        {
            var result = Query.From<TestDtoWithDefaultProperty>()
                .WhereAll()
                .Select(x => x.APropertyName)
                .OrderBy(x => x.APropertyName)
                .GetDefaultOrdering();

            result.Should().Be(nameof(TestDtoWithDefaultProperty.APropertyName));
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestDtoWithoutDefaultOrderingOrBackupProperty : IQueryableEntity
    {
        public string APropertyName { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestDtoWithoutDefaultOrderingProperty : IQueryableEntity
    {
        public string Id { get; set; }

        public string APropertyName { get; set; }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestDtoWithDefaultProperty : IQueryableEntity
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Id { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string LastPersistedAtUtc { get; set; }

        public string APropertyName { get; set; }
    }
}