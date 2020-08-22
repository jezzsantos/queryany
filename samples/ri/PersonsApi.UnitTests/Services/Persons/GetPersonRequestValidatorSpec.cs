using Api.Common.Validators;
using Api.Interfaces.ServiceOperations;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PersonsApi.Properties;
using PersonsApi.Services.Persons;
using ServiceStack.FluentValidation;

namespace PersonsApi.UnitTests.Services.Persons
{
    [TestClass, TestCategory("Unit")]
    public class GetPersonRequestValidatorSpec
    {
        private GetPersonRequest dto;
        private Mock<IIdentifierFactory> identifierFactory;
        private GetPersonRequestValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator = new GetPersonRequestValidator(new HasGetOptionsValidator(), this.identifierFactory.Object);
            this.dto = new GetPersonRequest
            {
                Id = "anid"
            };
        }

        [TestMethod]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenIdIsNull_ThenThrows()
        {
            this.dto.Id = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.AnyValidator_InvalidId);
        }

        [TestMethod]
        public void WhenIdIsInvalid_ThenThrows()
        {
            this.identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(false);

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.AnyValidator_InvalidId);
        }
    }
}