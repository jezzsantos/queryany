using System;
using Api.Common.Validators;
using Api.Interfaces.ServiceOperations;
using CarsApi.Properties;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;
using ServiceStack.FluentValidation;

namespace CarsApi.Services.Cars
{
    internal class OccupyCarRequestValidator : AbstractValidator<OccupyCarRequest>
    {
        public OccupyCarRequestValidator(IIdentifierFactory identifierFactory)
        {
            RuleFor(dto => dto.Id)
                .IsEntityId(identifierFactory)
                .WithMessage(Resources.AnyValidator_InvalidId);
            RuleFor(dto => dto.UntilUtc)
                .Must(dto => dto.HasValue())
                .WithMessage(Resources.OccupyCarRequestValidator_InvalidUntilUtc);
            When(dto => dto.UntilUtc.HasValue(), () =>
            {
                RuleFor(dto => dto.UntilUtc)
                    .InclusiveBetween(DateTime.UtcNow, DateTime.MaxValue)
                    .WithMessage(Resources.OccupyCarRequestValidator_PastUntilUtc);
            });
        }
    }
}