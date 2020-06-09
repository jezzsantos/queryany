using CarsApi.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Interfaces;
using ServiceStack.FluentValidation;
using Resources = CarsApi.Properties.Resources;

namespace CarsApi.UnitTests.Validators
{
    [TestClass]
    public class HasGetOptionsValdatorSpec
    {
        public static IAssertion Assert = new Assertion();

        private HasGetOptionsValidator validator;
        private HasGetOptionsDto dto;

        [TestInitialize]
        public void Initialize()
        {
            this.validator = new HasGetOptionsValidator();
            this.dto = new HasGetOptionsDto();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAllPropertiesValid_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsNull_ThenSucceeds()
        {
            this.dto.Embed = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsOff_ThenSucceeds()
        {
            this.dto.Embed = HasGetOptions.EmbedNone;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsTopLevelField_ThenSucceeds()
        {
            this.dto.Embed = "aresourceref";

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsChildLevelField_ThenSucceeds()
        {
            this.dto.Embed = "aresourceref.achildresourceref";

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsGrandChildLevelField_ThenSucceeds()
        {
            this.dto.Embed = "aresourceref.achildresourceref.agrandchildresourceref";

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsInvalidResourceReference_ThenThrows()
        {
            this.dto.Embed = "^aresourceref";

            Assert.Throws<ValidationException>(Resources.HasGetOptionsValidator_InvalidEmbed,
                () => this.validator.ValidateAndThrow(this.dto));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsInvalidChildResourceReference_ThenThrows()
        {
            this.dto.Embed = "aresourceref.^achildresourceref";

            Assert.Throws<ValidationException>(Resources.HasGetOptionsValidator_InvalidEmbed,
                () => this.validator.ValidateAndThrow(this.dto));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedIsInvalidGrandChildResourceReference_ThenThrows()
        {
            this.dto.Embed = "aresourceref.achildresourceref.^agrandchildresourceref";

            Assert.Throws<ValidationException>(Resources.HasGetOptionsValidator_InvalidEmbed,
                () => this.validator.ValidateAndThrow(this.dto));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenEmbedContainsTooManyResources_ThenThrows()
        {
            this.dto.Embed =
                "aresourceref1,aresourceref2,aresourceref3,aresourceref4,aresourceref5,aresourceref6,aresourceref7,aresourceref8,aresourceref9,aresourceref10,aresourceref11";

            Assert.Throws<ValidationException>(Resources.HasGetOptionsValidator_TooManyResourceReferences,
                () => this.validator.ValidateAndThrow(this.dto));
        }
    }

    internal class HasGetOptionsDto : IHasGetOptions
    {
        public string Embed { get; set; }
    }
}
