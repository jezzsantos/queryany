using CarsApi.Properties;
using CarsApi.Validators;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services.Interfaces;
using ServiceStack.FluentValidation;
using ServiceStack.FluentValidation.Results;

namespace CarsApi.UnitTests.Validators
{
    [TestClass]
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
                Filter = "afield1;afield2",
                Distinct = "adistinct"
            };
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAllPropertiesValid_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenLimitIsNull_ThenSucceeds()
        {
            this.dto.Limit = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenLimitIsMin_ThenSucceeds()
        {
            this.dto.Limit = SearchOptions.NoLimit;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenLimitIsLessThanMax_ThenSucceeds()
        {
            this.dto.Limit = SearchOptions.MaxLimit - 1;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenLimitIsLessThanMin_ThenThrows()
        {
            this.dto.Limit = SearchOptions.NoLimit - 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidLimit);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenLimitIsGreaterThanMax_ThenThrows()
        {
            this.dto.Limit = SearchOptions.MaxLimit + 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidLimit);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOffsetIsNull_ThenSucceeds()
        {
            this.dto.Offset = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOffsetIsMin_ThenSucceeds()
        {
            this.dto.Offset = SearchOptions.NoOffset;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOffsetIsLessThanMax_ThenSucceeds()
        {
            this.dto.Offset = SearchOptions.MaxLimit - 1;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOffsetIsLessThanMin_ThenThrows()
        {
            this.dto.Offset = SearchOptions.NoOffset - 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidOffset);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenOffsetIsGreaterThanMax_ThenThrows()
        {
            this.dto.Offset = SearchOptions.MaxLimit + 1;

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidOffset);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSortIsNull_ThenSucceeds()
        {
            this.dto.Sort = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenSortIsInvalid_ThenThrows()
        {
            this.dto.Sort = "*";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidSort);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFilterIsNull_ThenSucceeds()
        {
            this.dto.Filter = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenFilterIsInvalid_ThenThrows()
        {
            this.dto.Filter = "*";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidFilter);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenDistinctIsNull_ThenSucceeds()
        {
            this.dto.Distinct = null;

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenDistinctIsInvalid_ThenThrows()
        {
            this.dto.Distinct = "*";

            this.validator.Invoking(x => x.ValidateAndThrow(this.dto)).Should().Throw<ValidationException>()
                .WithMessageLike(Resources.HasSearchOptionsValidator_InvalidDistinct);
        }
    }

    public class HasSearchOptionsDto : IHasSearchOptions
    {
        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }

        public string Distinct { get; set; }

        public string Embed { get; set; }
    }
}