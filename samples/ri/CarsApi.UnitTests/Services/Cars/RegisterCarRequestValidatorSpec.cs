using Api.Interfaces.ServiceOperations.Cars;
using CarsApi.Properties;
using CarsApi.Services.Cars;
using CarsDomain;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace CarsApi.UnitTests.Services.Cars
{
    [Trait("Category", "Unit")]
    public class RegisterCarRequestValidatorSpec
    {
        private readonly RegisterCarRequest dto;
        private readonly RegisterCarRequestValidator validator;

        public RegisterCarRequestValidatorSpec()
        {
            var identifierFactory = new Mock<IIdentifierFactory>();
            identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator = new RegisterCarRequestValidator(identifierFactory.Object);
            this.dto = new RegisterCarRequest
            {
                Id = "anid",
                Jurisdiction = LicensePlate.Jurisdictions[0],
                Number = "ABC123"
            };
        }

        [Fact]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenJurisdictionIsNull_ThenThrows()
        {
            this.dto.Jurisdiction = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [Fact]
        public void WhenJurisdictionNotValid_ThenThrows()
        {
            this.dto.Jurisdiction = "invalid";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.RegisterCarRequestValidator_InvalidJurisdiction);
        }

        [Fact]
        public void WhenJurisdiction_ThenSucceeds()
        {
            this.dto.Jurisdiction = LicensePlate.Jurisdictions[0];

            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenNumberIsNull_ThenThrows()
        {
            this.dto.Number = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [Fact]
        public void WhenNumberNotValid_ThenThrows()
        {
            this.dto.Number = "##aninvalidnumber##";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.RegisterCarRequestValidator_InvalidNumber);
        }

        [Fact]
        public void WhenNumber_ThenSucceeds()
        {
            this.dto.Number = "ABC123";

            this.validator.ValidateAndThrow(this.dto);
        }
    }
}