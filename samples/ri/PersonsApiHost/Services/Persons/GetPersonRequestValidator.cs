using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.Persons;
using Common;
using Domain.Interfaces.Entities;
using PersonsApiHost.Properties;
using ServiceStack.FluentValidation;

namespace PersonsApiHost.Services.Persons
{
    internal class GetPersonRequestValidator : AbstractValidator<GetPersonRequest>
    {
        public GetPersonRequestValidator(IHasGetOptionsValidator hasGetOptionsValidator,
            IIdentifierFactory identifierFactory)
        {
            hasGetOptionsValidator.GuardAgainstNull(nameof(hasGetOptionsValidator));

            Include(hasGetOptionsValidator);
            RuleFor(dto => dto.Id).IsEntityId(identifierFactory)
                .WithMessage(Resources.AnyValidator_InvalidId);
        }
    }
}