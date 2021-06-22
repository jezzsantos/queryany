using System.Linq;
using FluentAssertions;
using Xunit;

namespace Application.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class HasGetOptionsSpec
    {
        [Fact]
        public void WhenAll_ThenReturnsAll()
        {
            var result = HasGetOptions.All.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.All);
        }

        [Fact]
        public void WhenNone_ThenReturnsNone()
        {
            var result = HasGetOptions.None.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.None);
        }

        [Fact]
        public void WhenCustomWithSingleResourceReference_ThenReturnsChildResources()
        {
            var result = HasGetOptions.Custom<TestResource>(x => x.AProperty1).ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.Custom);
            result.ResourceReferences.Count().Should().Be(1);
            result.ResourceReferences.ToList()[0].Should().Be("testresource.aproperty1");
        }

        [Fact]
        public void WhenCustomWithMultipleResourceReferences_ThenReturnsChildResources()
        {
            var result = HasGetOptions.Custom<TestResource>(x => x.AProperty1, x => x.AProperty2).ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.Custom);
            result.ResourceReferences.Count().Should().Be(2);
            result.ResourceReferences.ToList()[0].Should().Be("testresource.aproperty1");
            result.ResourceReferences.ToList()[1].Should().Be("testresource.aproperty2");
        }
    }

    [Trait("Category", "Unit")]
    public class HasGetOptionsExtensionsSpec
    {
        private readonly GetOptionsDto hasGetOptions;

        public HasGetOptionsExtensionsSpec()
        {
            this.hasGetOptions = new GetOptionsDto();
        }

        [Fact]
        public void WhenToGetOptionsAndNullOptions_ThenReturnsNull()
        {
            var result = ((GetOptionsDto) null).ToGetOptions();

            result.Should().BeNull();
        }

        [Fact]
        public void WhenToGetOptionsAndEmbedIsUndefined_ThenReturnsEnabled()
        {
            this.hasGetOptions.Embed = null;

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.All);
            result.ResourceReferences.Count().Should().Be(0);
        }

        [Fact]
        public void WhenToGetOptionsAndEmbedIsOff_ThenReturnsDisabled()
        {
            this.hasGetOptions.Embed = HasGetOptions.EmbedNone;

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.None);
            result.ResourceReferences.Count().Should().Be(0);
        }

        [Fact]
        public void WhenToGetOptionsAndEmbedIsAll_ThenReturnsEnabled()
        {
            this.hasGetOptions.Embed = HasGetOptions.EmbedAll;

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.All);
            result.ResourceReferences.Count().Should().Be(0);
        }

        [Fact]
        public void WhenToGetOptionsAndEmbedIsCommaDelimitedResourceReferences_ThenReturnsChildResources()
        {
            this.hasGetOptions.Embed = "aresourceref1, aresourceref2, aresourceref3,,,";

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.Custom);
            result.ResourceReferences.Count().Should().Be(3);
            result.ResourceReferences.ToList()[0].Should().Be("aresourceref1");
            result.ResourceReferences.ToList()[1].Should().Be("aresourceref2");
            result.ResourceReferences.ToList()[2].Should().Be("aresourceref3");
        }
    }

    public class TestResource
    {
        public string AProperty1 { get; set; }

        public string AProperty2 { get; set; }
    }

    public class GetOptionsDto : IHasGetOptions
    {
        public string Embed { get; set; }
    }
}