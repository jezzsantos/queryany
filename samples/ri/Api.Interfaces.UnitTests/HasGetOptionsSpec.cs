﻿using System.Linq;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Api.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class HasGetOptionsSpec
    {
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void WhenAll_ThenReturnsAll()
        {
            var result = HasGetOptions.All.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.All);
        }

        [TestMethod]
        public void WhenNone_ThenReturnsNone()
        {
            var result = HasGetOptions.None.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.None);
        }

        [TestMethod]
        public void WhenCustomWithSingleResourceReference_ThenReturnsChildResources()
        {
            var result = HasGetOptions.Custom<TestResource>(x => x.AProperty1).ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.Custom);
            Assert.AreEqual(1, result.ResourceReferences.Count());
            Assert.AreEqual("testresource.aproperty1", result.ResourceReferences.ToList()[0]);
        }

        [TestMethod]
        public void WhenCustomWithMultipleResourceReferences_ThenReturnsChildResources()
        {
            var result = HasGetOptions.Custom<TestResource>(x => x.AProperty1, x => x.AProperty2).ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.Custom);
            Assert.AreEqual(2, result.ResourceReferences.Count());
            Assert.AreEqual("testresource.aproperty1", result.ResourceReferences.ToList()[0]);
            Assert.AreEqual("testresource.aproperty2", result.ResourceReferences.ToList()[1]);
        }
    }

    [TestClass, TestCategory("Unit")]
    public class HasGetOptionsExtensionsSpec
    {
        private GetOptionsDto hasGetOptions;

        [TestInitialize]
        public void Initialize()
        {
            this.hasGetOptions = new GetOptionsDto();
        }

        [TestMethod]
        public void WhenToGetOptionsAndNullOptions_ThenReturnsNull()
        {
            var result = ((GetOptionsDto) null).ToGetOptions();

            Assert.IsNull(result);
        }

        [TestMethod]
        public void WhenToGetOptionsAndEmbedIsUndefined_ThenReturnsEnabled()
        {
            this.hasGetOptions.Embed = null;

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.All);
            Assert.AreEqual(0, result.ResourceReferences.Count());
        }

        [TestMethod]
        public void WhenToGetOptionsAndEmbedIsOff_ThenReturnsDisabled()
        {
            this.hasGetOptions.Embed = HasGetOptions.EmbedNone;

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.None);
            Assert.AreEqual(0, result.ResourceReferences.Count());
        }

        [TestMethod]
        public void WhenToGetOptionsAndEmbedIsAll_ThenReturnsEnabled()
        {
            this.hasGetOptions.Embed = HasGetOptions.EmbedAll;

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.All);
            Assert.AreEqual(0, result.ResourceReferences.Count());
        }

        [TestMethod]
        public void WhenToGetOptionsAndEmbedIsCommaDelimitedResourceReferences_ThenReturnsChildResources()
        {
            this.hasGetOptions.Embed = "aresourceref1, aresourceref2, aresourceref3,,,";

            var result = this.hasGetOptions.ToGetOptions();

            result.Expand.Should().Be(ExpandOptions.Custom);
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