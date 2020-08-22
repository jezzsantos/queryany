using System;
using Api.Interfaces.ServiceOperations;
using CarsApi.Properties;
using CarsApi.Services.Cars;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceStack.FluentValidation;

namespace CarsApi.UnitTests.Services.Cars
{
    [TestClass, TestCategory("Unit")]
    public class OccupyCarRequestValidatorSpec
    {
        private OccupyCarRequest dto;
        private Mock<IIdentifierFactory> identifierFactory;
        private OccupyCarRequestValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator = new OccupyCarRequestValidator(this.identifierFactory.Object);
            this.dto = new OccupyCarRequest
            {
                Id = "anid",
                UntilUtc = DateTime.UtcNow.AddSeconds(1)
            };
        }

        [TestMethod]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenUntilUtcIsMin_ThenThrows()
        {
            this.dto.UntilUtc = DateTime.MinValue;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.OccupyCarRequestValidator_InvalidUntilUtc);
        }

        [TestMethod]
        public void WhenUntilUtcInPast_ThenThrows()
        {
            this.dto.UntilUtc = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(1));

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.OccupyCarRequestValidator_PastUntilUtc);
        }

        [TestMethod]
        public void WhenUntilUtcIsFuture_ThenSucceeds()
        {
            this.dto.UntilUtc = DateTime.UtcNow.AddSeconds(1);

            this.validator.ValidateAndThrow(this.dto);
        }
    }
}