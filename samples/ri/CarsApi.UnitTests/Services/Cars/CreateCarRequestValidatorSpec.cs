using Api.Interfaces.ServiceOperations.Cars;
using CarsApi.Properties;
using CarsApi.Services.Cars;
using CarsDomain;
using FluentAssertions;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace CarsApi.UnitTests.Services.Cars
{
    [Trait("Category", "Unit")]
    public class CreateCarRequestValidatorSpec
    {
        private readonly CreateCarRequest dto;
        private readonly CreateCarRequestValidator validator;

        public CreateCarRequestValidatorSpec()
        {
            this.validator = new CreateCarRequestValidator();
            this.dto = new CreateCarRequest
            {
                Year = Manufacturer.MinYear,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            };
        }

        [Fact]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenYearIsZero_ThenThrows()
        {
            this.dto.Year = 0;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidYear);
        }

        [Fact]
        public void WhenYearIsTooOld_ThenThrows()
        {
            this.dto.Year = Manufacturer.MinYear - 1;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidYear);
        }

        [Fact]
        public void WhenYearIsTooNew_ThenThrows()
        {
            this.dto.Year = Manufacturer.MaxYear + 1;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidYear);
        }

        [Fact]
        public void WhenMakeIsNull_ThenThrows()
        {
            this.dto.Make = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [Fact]
        public void WhenMakeIsUnknown_ThenThrows()
        {
            this.dto.Make = "unknownmake";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidMake);
        }

        [Fact]
        public void WhenModelIsNull_ThenThrows()
        {
            this.dto.Model = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [Fact]
        public void WhenModelIsUnknown_ThenThrows()
        {
            this.dto.Model = "unknownmake";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidModel);
        }
    }
}