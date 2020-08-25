using System;
using Api.Common.Validators;
using Api.Interfaces.ServiceOperations;
using CarsApi.Properties;
using QueryAny.Primitives;
using ServiceStack.FluentValidation;

namespace CarsApi.Services.Cars
{
    internal class SearchAvailableCarsRequestValidator : AbstractValidator<SearchAvailableCarsRequest>
    {
        public SearchAvailableCarsRequestValidator(IHasSearchOptionsValidator hasSearchOptionsValidator)
        {
            hasSearchOptionsValidator.GuardAgainstNull(nameof(hasSearchOptionsValidator));

            Include(hasSearchOptionsValidator);

            When(dto => dto.FromUtc.HasValue, () =>
            {
                RuleFor(dto => dto.FromUtc)
                    .Must(dto => dto.GetValueOrDefault().HasValue())
                    .WithMessage(Resources.SearchAvailableCarsRequestValidator_InvalidFrom);
                RuleFor(dto => dto.FromUtc)
                    .InclusiveBetween(DateTime.UtcNow, DateTime.MaxValue)
                    .WithMessage(Resources.SearchAvailableCarsRequestValidator_PastFrom);
                RuleFor(dto => dto.FromUtc)
                    .LessThan(dto => dto.ToUtc)
                    .WithMessage(Resources.SearchAvailableCarsRequestValidator_FromAfterTo);
            });
            When(dto => dto.ToUtc.HasValue, () =>
            {
                RuleFor(dto => dto.ToUtc)
                    .Must(dto => dto.GetValueOrDefault().HasValue())
                    .WithMessage(Resources.SearchAvailableCarsRequestValidator_InvalidTo);
                RuleFor(dto => dto.ToUtc)
                    .InclusiveBetween(DateTime.UtcNow, DateTime.MaxValue)
                    .WithMessage(Resources.SearchAvailableCarsRequestValidator_PastTo);
            });
        }
    }
}