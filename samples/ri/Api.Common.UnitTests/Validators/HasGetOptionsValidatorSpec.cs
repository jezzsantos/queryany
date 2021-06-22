using Api.Common.Properties;
using Api.Common.Validators;
using Application.Interfaces;
using FluentAssertions;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace Api.Common.UnitTests.Validators
{
    [Trait("Category", "Unit")]
    public class HasGetOptionsValidatorSpec
    {
        private readonly HasGetOptionsDto dto;
        private readonly HasGetOptionsValidator validator;

        public HasGetOptionsValidatorSpec()
        {
            this.validator = new HasGetOptionsValidator();
            this.dto = new HasGetOptionsDto();
        }

        [Fact]
        public void WhenAllPropertiesValid_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenEmbedIsNull_ThenSucceeds()
        {
            this.dto.Embed = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenEmbedIsOff_ThenSucceeds()
        {
            this.dto.Embed = HasGetOptions.EmbedNone;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenEmbedIsTopLevelField_ThenSucceeds()
        {
            this.dto.Embed = "aresourceref";

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenEmbedIsChildLevelField_ThenSucceeds()
        {
            this.dto.Embed = "aresourceref.achildresourceref";

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenEmbedIsGrandChildLevelField_ThenSucceeds()
        {
            this.dto.Embed = "aresourceref.achildresourceref.agrandchildresourceref";

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenEmbedIsInvalidResourceReference_ThenThrows()
        {
            this.dto.Embed = "^aresourceref";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasGetOptionsValidator_InvalidEmbed);
        }

        [Fact]
        public void WhenEmbedIsInvalidChildResourceReference_ThenThrows()
        {
            this.dto.Embed = "aresourceref.^achildresourceref";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasGetOptionsValidator_InvalidEmbed);
        }

        [Fact]
        public void WhenEmbedIsInvalidGrandChildResourceReference_ThenThrows()
        {
            this.dto.Embed = "aresourceref.achildresourceref.^agrandchildresourceref";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasGetOptionsValidator_InvalidEmbed);
        }

        [Fact]
        public void WhenEmbedContainsTooManyResources_ThenThrows()
        {
            this.dto.Embed =
                "aresourceref1,aresourceref2,aresourceref3,aresourceref4,aresourceref5,aresourceref6,aresourceref7,aresourceref8,aresourceref9,aresourceref10,aresourceref11";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasGetOptionsValidator_TooManyResourceReferences);
        }
    }

    internal class HasGetOptionsDto : IHasGetOptions
    {
        public string Embed { get; set; }
    }
}