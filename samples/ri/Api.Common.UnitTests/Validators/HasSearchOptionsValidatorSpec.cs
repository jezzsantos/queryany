using Api.Common.Properties;
using Api.Common.Validators;
using Application.Interfaces;
using FluentAssertions;
using Moq;
using ServiceStack.FluentValidation;
using ServiceStack.FluentValidation.Results;
using UnitTesting.Common;
using Xunit;

namespace Api.Common.UnitTests.Validators
{
    [Trait("Category", "Unit")]
    public class HasSearchOptionsValidatorSpec
    {
        private readonly HasSearchOptionsDto dto;
        private readonly HasSearchOptionsValidator validator;

        public HasSearchOptionsValidatorSpec()
        {
            var getOptionsValidator = new Mock<IHasGetOptionsValidator>();
            getOptionsValidator.Setup(gv => gv.Validate(It.IsAny<IValidationContext>()))
                .Returns(new ValidationResult());
            this.validator = new HasSearchOptionsValidator(getOptionsValidator.Object);
            this.dto = new HasSearchOptionsDto
            {
                Limit = 0,
                Offset = 0,
                Sort = "+afield",
                Filter = "afield1;afield2"
            };
        }

        [Fact]
        public void WhenAllPropertiesValid_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenLimitIsNull_ThenSucceeds()
        {
            this.dto.Limit = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenLimitIsMin_ThenSucceeds()
        {
            this.dto.Limit = SearchOptions.NoLimit;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenLimitIsLessThanMax_ThenSucceeds()
        {
            this.dto.Limit = SearchOptions.MaxLimit - 1;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenLimitIsLessThanMin_ThenThrows()
        {
            this.dto.Limit = SearchOptions.NoLimit - 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidLimit);
        }

        [Fact]
        public void WhenLimitIsGreaterThanMax_ThenThrows()
        {
            this.dto.Limit = SearchOptions.MaxLimit + 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidLimit);
        }

        [Fact]
        public void WhenOffsetIsNull_ThenSucceeds()
        {
            this.dto.Offset = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenOffsetIsMin_ThenSucceeds()
        {
            this.dto.Offset = SearchOptions.NoOffset;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenOffsetIsLessThanMax_ThenSucceeds()
        {
            this.dto.Offset = SearchOptions.MaxLimit - 1;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenOffsetIsLessThanMin_ThenThrows()
        {
            this.dto.Offset = SearchOptions.NoOffset - 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidOffset);
        }

        [Fact]
        public void WhenOffsetIsGreaterThanMax_ThenThrows()
        {
            this.dto.Offset = SearchOptions.MaxLimit + 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidOffset);
        }

        [Fact]
        public void WhenSortIsNull_ThenSucceeds()
        {
            this.dto.Sort = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenSortIsInvalid_ThenThrows()
        {
            this.dto.Sort = "*";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidSort);
        }

        [Fact]
        public void WhenFilterIsNull_ThenSucceeds()
        {
            this.dto.Filter = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenFilterIsInvalid_ThenThrows()
        {
            this.dto.Filter = "*";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidFilter);
        }
    }

    public class HasSearchOptionsDto : IHasSearchOptions
    {
        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }

        public string Embed { get; set; }
    }
}