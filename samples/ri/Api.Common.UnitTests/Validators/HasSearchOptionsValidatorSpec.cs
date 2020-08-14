using Api.Common.Properties;
using Api.Common.Validators;
using Api.Interfaces;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceStack.FluentValidation;
using ServiceStack.FluentValidation.Results;

namespace Api.Common.UnitTests.Validators
{
    [TestClass, TestCategory("Unit")]
    public class HasSearchOptionsValidatorSpec
    {
        private HasSearchOptionsDto dto;
        private Mock<IHasGetOptionsValidator> getOptionsValidator;
        private HasSearchOptionsValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            this.getOptionsValidator = new Mock<IHasGetOptionsValidator>();
            this.getOptionsValidator.Setup(gv => gv.Validate(It.IsAny<ValidationContext>()))
                .Returns(new ValidationResult());
            this.validator = new HasSearchOptionsValidator(this.getOptionsValidator.Object);
            this.dto = new HasSearchOptionsDto
            {
                Limit = 0,
                Offset = 0,
                Sort = "+afield",
                Filter = "afield1;afield2"
            };
        }

        [TestMethod]
        public void WhenAllPropertiesValid_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenLimitIsNull_ThenSucceeds()
        {
            this.dto.Limit = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenLimitIsMin_ThenSucceeds()
        {
            this.dto.Limit = SearchOptions.NoLimit;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenLimitIsLessThanMax_ThenSucceeds()
        {
            this.dto.Limit = SearchOptions.MaxLimit - 1;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenLimitIsLessThanMin_ThenThrows()
        {
            this.dto.Limit = SearchOptions.NoLimit - 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidLimit);
        }

        [TestMethod]
        public void WhenLimitIsGreaterThanMax_ThenThrows()
        {
            this.dto.Limit = SearchOptions.MaxLimit + 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidLimit);
        }

        [TestMethod]
        public void WhenOffsetIsNull_ThenSucceeds()
        {
            this.dto.Offset = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenOffsetIsMin_ThenSucceeds()
        {
            this.dto.Offset = SearchOptions.NoOffset;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenOffsetIsLessThanMax_ThenSucceeds()
        {
            this.dto.Offset = SearchOptions.MaxLimit - 1;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenOffsetIsLessThanMin_ThenThrows()
        {
            this.dto.Offset = SearchOptions.NoOffset - 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidOffset);
        }

        [TestMethod]
        public void WhenOffsetIsGreaterThanMax_ThenThrows()
        {
            this.dto.Offset = SearchOptions.MaxLimit + 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidOffset);
        }

        [TestMethod]
        public void WhenSortIsNull_ThenSucceeds()
        {
            this.dto.Sort = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenSortIsInvalid_ThenThrows()
        {
            this.dto.Sort = "*";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidSort);
        }

        [TestMethod]
        public void WhenFilterIsNull_ThenSucceeds()
        {
            this.dto.Filter = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
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