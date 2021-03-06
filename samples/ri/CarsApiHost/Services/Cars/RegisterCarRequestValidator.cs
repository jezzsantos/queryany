﻿using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.Cars;
using CarsApiHost.Properties;
using CarsDomain;
using Common;
using Domain.Interfaces.Entities;
using ServiceStack.FluentValidation;

namespace CarsApiHost.Services.Cars
{
    internal class RegisterCarRequestValidator : AbstractValidator<RegisterCarRequest>
    {
        public RegisterCarRequestValidator(IIdentifierFactory identifierFactory)
        {
            RuleFor(dto => dto.Id)
                .IsEntityId(identifierFactory)
                .WithMessage(Resources.AnyValidator_InvalidId);
            RuleFor(dto => dto.Jurisdiction)
                .NotEmpty();
            When(dto => dto.Jurisdiction.HasValue(), () =>
            {
                RuleFor(dto => dto.Jurisdiction)
                    .Matches(Validations.Car.Jurisdiction.Expression)
                    .Must(dto => LicensePlate.Jurisdictions.Contains(dto))
                    .WithMessage(Resources.RegisterCarRequestValidator_InvalidJurisdiction);
            });
            RuleFor(dto => dto.Number)
                .NotEmpty();
            When(dto => dto.Number.HasValue(), () =>
            {
                RuleFor(dto => dto.Number)
                    .Matches(Validations.Car.Number.Expression)
                    .WithMessage(Resources.RegisterCarRequestValidator_InvalidNumber);
            });
        }
    }
}