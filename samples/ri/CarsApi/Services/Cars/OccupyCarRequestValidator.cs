using CarsApi.Properties;
using QueryAny.Primitives;
using Services.Interfaces.Apis;
using ServiceStack.FluentValidation;
using Storage.Interfaces;

namespace CarsApi.Services.Cars
{
    internal class OccupyCarRequestValidator : AbstractValidator<OccupyCarRequest>
    {
        public OccupyCarRequestValidator(IIdentifierFactory identifierFactory)
        {
            RuleFor(dto => dto.Id).Must(identifierFactory.IsValid)
                .WithMessage(Resources.AnyValidator_InvalidId);
            RuleFor(dto => dto.UntilUtc).Must(dto => dto.HasValue())
                .WithMessage(Resources.OccupyCarRequestValidator_InvalidUntilUtc);
        }
    }
}