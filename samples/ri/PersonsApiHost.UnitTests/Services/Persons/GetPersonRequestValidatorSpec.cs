using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.Persons;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using PersonsApiHost.Properties;
using PersonsApiHost.Services.Persons;
using ServiceStack.FluentValidation;
using UnitTesting.Common;
using Xunit;

namespace PersonsApiHost.UnitTests.Services.Persons
{
    [Trait("Category", "Unit")]
    public class GetPersonRequestValidatorSpec
    {
        private readonly GetPersonRequest dto;
        private readonly Mock<IIdentifierFactory> identifierFactory;
        private readonly GetPersonRequestValidator validator;

        public GetPersonRequestValidatorSpec()
        {
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.IsValid(It.IsAny<Identifier>())).Returns(true);
            this.validator = new GetPersonRequestValidator(new HasGetOptionsValidator(), this.identifierFactory.Object);
            this.dto = new GetPersonRequest
            {
                Id = "anid"
            };
        }

        [Fact]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [Fact]
        public void WhenIdIsNull_ThenThrows()
        {
            this.dto.Id = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.AnyValidator_InvalidId);
        }

        [Fact]
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