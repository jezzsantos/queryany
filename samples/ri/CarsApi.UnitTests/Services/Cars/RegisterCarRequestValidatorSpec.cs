using Api.Interfaces.ServiceOperations;
using CarsApi.Properties;
using CarsApi.Services.Cars;
using CarsDomain;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceStack.FluentValidation;

namespace CarsApi.UnitTests.Services.Cars
{
    [TestClass, TestCategory("Unit")]
    public class RegisterCarRequestValidatorSpec
    {
        private RegisterCarRequest dto;
        private Mock<IIdentifierFactory> identifierFactory;
        private RegisterCarRequestValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator = new RegisterCarRequestValidator(this.identifierFactory.Object);
            this.dto = new RegisterCarRequest
            {
                Id = "anid",
                Jurisdiction = LicensePlate.Jurisdictions[0],
                Number = "ABC123"
            };
        }

        [TestMethod]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenJurisdictionIsNull_ThenThrows()
        {
            this.dto.Jurisdiction = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [TestMethod]
        public void WhenJurisdictionNotValid_ThenThrows()
        {
            this.dto.Jurisdiction = "invalid";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.RegisterCarRequestValidator_InvalidJurisdiction);
        }

        [TestMethod]
        public void WhenJurisdiction_ThenSucceeds()
        {
            this.dto.Jurisdiction = LicensePlate.Jurisdictions[0];

            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenNumberIsNull_ThenThrows()
        {
            this.dto.Number = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [TestMethod]
        public void WhenNumberNotValid_ThenThrows()
        {
            this.dto.Number = "##aninvalidnumber##";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.RegisterCarRequestValidator_InvalidNumber);
        }

        [TestMethod]
        public void WhenNumber_ThenSucceeds()
        {
            this.dto.Number = "ABC123";

            this.validator.ValidateAndThrow(this.dto);
        }
    }
}