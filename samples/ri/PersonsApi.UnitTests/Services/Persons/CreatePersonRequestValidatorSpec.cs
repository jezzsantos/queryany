using Api.Interfaces.ServiceOperations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonsApi.Properties;
using PersonsApi.Services.Persons;
using ServiceStack.FluentValidation;

namespace PersonsApi.UnitTests.Services.Persons
{
    [TestClass, TestCategory("Unit")]
    public class CreatePersonRequestValidatorSpec
    {
        private CreatePersonRequest dto;
        private CreatePersonRequestValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            this.validator = new CreatePersonRequestValidator();
            this.dto = new CreatePersonRequest
            {
                FirstName = "afirstname",
                LastName = "alastname"
            };
        }

        [TestMethod]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenFirstNameIsNull_ThenThrows()
        {
            this.dto.FirstName = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [TestMethod]
        public void WhenFirstNameIsInvalid_ThenThrows()
        {
            this.dto.FirstName = "^invalid^";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreatePersonRequestValidator_InvalidFirstName);
        }

        [TestMethod]
        public void WhenLastNameIsNull_ThenThrows()
        {
            this.dto.LastName = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [TestMethod]
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