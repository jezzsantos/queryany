using Api.Interfaces.ServiceOperations.Persons;
using Common;
using PersonsApiHost.Properties;
using PersonsDomain;
using ServiceStack.FluentValidation;

namespace PersonsApiHost.Services.Persons
{
    internal class CreatePersonRequestValidator : AbstractValidator<CreatePersonRequest>
    {
        public CreatePersonRequestValidator()
        {
            RuleFor(dto => dto.FirstName)
                .NotEmpty();
            When(dto => dto.FirstName.HasValue(), () =>
            {
                RuleFor(dto => dto.FirstName)
                    .Matches(Validations.Person.Name.Expression)
                    .WithMessage(Resources.CreatePersonRequestValidator_InvalidFirstName);
            });
            RuleFor(dto => dto.LastName)
                .NotEmpty();
            When(dto => dto.LastName.HasValue(), () =>
            {
                RuleFor(dto => dto.LastName)
                    .Matches(Validations.Person.Name.Expression)
                    .WithMessage(Resources.CreatePersonRequestValidator_InvalidLastName);
            });
        }
    }
}