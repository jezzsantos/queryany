using System;
using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.Cars;
using CarsApiHost.Properties;
using CarsApiHost.Services.Cars;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace CarsApiHost.UnitTests.Services.Cars
{
    [Trait("Category", "Unit")]
    public class SearchAvailableCarsRequestValidatorSpec
    {
        private readonly SearchAvailableCarsRequest dto;
        private readonly SearchAvailableCarsRequestValidator validator;

        public SearchAvailableCarsRequestValidatorSpec()
        {
            var identifierFactory = new Mock<IIdentifierFactory>();
            identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator =
                new SearchAvailableCarsRequestValidator(new HasSearchOptionsValidator(new HasGetOptionsValidator()));
            this.dto = new SearchAvailableCarsRequest
            {
                FromUtc = null,
                ToUtc = null
            };
        }

        [Fact]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenFromIsMin_ThenThrows()
        {
            this.dto.FromUtc = DateTime.MinValue;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.SearchAvailableCarsRequestValidator_InvalidFrom);
        }

        [Fact]
        public void WhenFromInPast_ThenThrows()
        {
            this.dto.FromUtc = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1));

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.SearchAvailableCarsRequestValidator_PastFrom);
        }

        [Fact]
        public void WhenFromIsGreaterThanTo_ThenThrows()
        {
            this.dto.FromUtc = DateTime.UtcNow.AddSeconds(1);
            this.dto.ToUtc = DateTime.UtcNow;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.SearchAvailableCarsRequestValidator_FromAfterTo);
        }

        [Fact]
        public void WhenToIsMin_ThenThrows()
        {
            this.dto.ToUtc = DateTime.MinValue;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.SearchAvailableCarsRequestValidator_InvalidTo);
        }

        [Fact]
        public void WhenToInPast_ThenThrows()
        {
            this.dto.ToUtc = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(1));

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.SearchAvailableCarsRequestValidator_PastTo);
        }

        [Fact]
        public void WhenToIsFuture_ThenSucceeds()
        {
            this.dto.ToUtc = DateTime.UtcNow.AddSeconds(1);

            this.validator.ValidateAndThrow(this.dto);
        }
    }
}