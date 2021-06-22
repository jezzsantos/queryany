using Api.Interfaces.ServiceOperations.Persons;
using FluentAssertions;
using PersonsApi.Properties;
using PersonsApi.Services.Persons;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace PersonsApi.UnitTests.Services.Persons
{
    [Trait("Category", "Unit")]
    public class CreatePersonRequestValidatorSpec
    {
        private readonly CreatePersonRequest dto;
        private readonly CreatePersonRequestValidator validator;

        public CreatePersonRequestValidatorSpec()
        {
            this.validator = new CreatePersonRequestValidator();
            this.dto = new CreatePersonRequest
            {
                FirstName = "afirstname",
                LastName = "alastname"
            };
        }

        [Fact]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenFirstNameIsNull_ThenThrows()
        {
            this.dto.FirstName = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [Fact]
        public void WhenFirstNameIsInvalid_ThenThrows()
        {
            this.dto.FirstName = "^invalid^";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreatePersonRequestValidator_InvalidFirstName);
        }

        [Fact]
        public void WhenLastNameIsNull_ThenThrows()
        {
            this.dto.LastName = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [Fact]
        public void WhenLastNameIsInvalid_ThenThrows()
        {
            this.dto.LastName = "^invalid^";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreatePersonRequestValidator_InvalidLastName);
        }
    }
}