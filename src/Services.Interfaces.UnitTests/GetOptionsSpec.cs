using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Services.Interfaces.UnitTests
{
    [TestClass]
    public class GetOptionsSpec
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAll_ThenReturnsAll()
        {
            var result = HasGetOptions.All.ToGetOptions();

            Assert.AreEqual(ExpandOptions.All, result.Expand);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenNone_ThenReturnsNone()
        {
            var result = HasGetOptions.None.ToGetOptions();

            Assert.AreEqual(ExpandOptions.None, result.Expand);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCustomWithSingleResourceReference_ThenReturnsChildResources()
        {
            var result = HasGetOptions.Custom<TestResource>(x => x.AProperty1).ToGetOptions();

            Assert.AreEqual(ExpandOptions.Custom, result.Expand);
            Assert.AreEqual(1, result.ResourceReferences.Count());
            Assert.AreEqual("testresource.aproperty1", result.ResourceReferences.ToList()[0]);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCustomWithMultipleResourceReferences_ThenReturnsChildResources()
        {
            var result = HasGetOptions.Custom<TestResource>(x => x.AProperty1, x => x.AProperty2).ToGetOptions();

            Assert.AreEqual(ExpandOptions.Custom, result.Expand);
            Assert.AreEqual(2, result.ResourceReferences.Count());
            Assert.AreEqual("testresource.aproperty1", result.ResourceReferences.ToList()[0]);
            Assert.AreEqual("testresource.aproperty2", result.ResourceReferences.ToList()[1]);
        }
    }

    [TestClass]
    public class IHasGetOptionsExtensionsSpec
    {
        private GetOptionsDto hasGetOptions;

        [TestInitialize]
        public void Initialize()
        {
            this.hasGetOptions = new GetOptionsDto();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToGetOptionsAndNullOptions_ThenReturnsNull()
        {
            var result = ((GetOptionsDto)null).ToGetOptions();

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToGetOptionsAndEmbedIsUndefined_ThenReturnsEnabled()
        {
            this.hasGetOptions.Embed = null;

            var result = this.hasGetOptions.ToGetOptions();

            Assert.AreEqual(ExpandOptions.All, result.Expand);
            Assert.AreEqual(0, result.ResourceReferences.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToGetOptionsAndEmbedIsOff_ThenReturnsDisabled()
        {
            this.hasGetOptions.Embed = HasGetOptions.EmbedNone;

            var result = this.hasGetOptions.ToGetOptions();

            Assert.AreEqual(ExpandOptions.None, result.Expand);
            Assert.AreEqual(0, result.ResourceReferences.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToGetOptionsAndEmbedIsAll_ThenReturnsEnabled()
        {
            this.hasGetOptions.Embed = HasGetOptions.EmbedAll;

            var result = this.hasGetOptions.ToGetOptions();

            Assert.AreEqual(ExpandOptions.All, result.Expand);
            Assert.AreEqual(0, result.ResourceReferences.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenToGetOptionsAndEmbedIsCommaDelimitedResourceReferences_ThenReturnsChildResources()
        {
            this.hasGetOptions.Embed = "aresourceref1, aresourceref2, aresourceref3,,,";

            var result = this.hasGetOptions.ToGetOptions();

            Assert.AreEqual(ExpandOptions.Custom, result.Expand);
            Assert.AreEqual(3, result.ResourceReferences.Count());
            Assert.AreEqual("aresourceref1", result.ResourceReferences.ToList()[0]);
            Assert.AreEqual("aresourceref2", result.ResourceReferences.ToList()[1]);
            Assert.AreEqual("aresourceref3", result.ResourceReferences.ToList()[2]);
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
