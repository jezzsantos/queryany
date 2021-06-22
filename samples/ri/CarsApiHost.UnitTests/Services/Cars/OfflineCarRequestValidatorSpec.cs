using System;
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
    public class OfflineCarRequestValidatorSpec
    {
        private readonly OfflineCarRequest dto;
        private readonly OfflineCarRequestValidator validator;

        public OfflineCarRequestValidatorSpec()
        {
            var identifierFactory = new Mock<IIdentifierFactory>();
            identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator = new OfflineCarRequestValidator(identifierFactory.Object);
            this.dto = new OfflineCarRequest
            {
                Id = "anid",
                FromUtc = DateTime.UtcNow.AddSeconds(1),
                ToUtc = DateTime.UtcNow.AddSeconds(2)
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
                .WithValidationMessageLike(Resources.OfflineCarRequestValidator_InvalidFrom);
        }

        [Fact]
        public void WhenFromInPast_ThenThrows()
        {
            this.dto.FromUtc = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(1));

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.OfflineCarRequestValidator_PastFrom);
        }

        [Fact]
        public void WhenFromIsGreaterThanTo_ThenThrows()
        {
            this.dto.FromUtc = DateTime.UtcNow.AddSeconds(1);
            this.dto.ToUtc = DateTime.UtcNow;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.OfflineCarRequestValidator_FromAfterTo);
        }

        [Fact]
        public void WhenToIsMin_ThenThrows()
        {
            this.dto.ToUtc = DateTime.MinValue;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.OfflineCarRequestValidator_InvalidTo);
        }

        [Fact]
        public void WhenToInPast_ThenThrows()
        {
            this.dto.ToUtc = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(1));

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.OfflineCarRequestValidator_PastTo);
        }

        [Fact]
        public void WhenToIsFuture_ThenSucceeds()
        {
            this.dto.ToUtc = DateTime.UtcNow.AddSeconds(1);

            this.validator.ValidateAndThrow(this.dto);
        }
    }
}