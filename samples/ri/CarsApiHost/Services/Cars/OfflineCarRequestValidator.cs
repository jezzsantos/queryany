﻿using System;
using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.Cars;
using CarsApiHost.Properties;
using Common;
using Domain.Interfaces.Entities;
using ServiceStack.FluentValidation;

namespace CarsApiHost.Services.Cars
{
    internal class OfflineCarRequestValidator : AbstractValidator<OfflineCarRequest>
    {
        public OfflineCarRequestValidator(IIdentifierFactory identifierFactory)
        {
            RuleFor(dto => dto.Id)
                .IsEntityId(identifierFactory)
                .WithMessage(Resources.AnyValidator_InvalidId);

            RuleFor(dto => dto.FromUtc)
                .Must(dto => dto.HasValue())
                .WithMessage(Resources.OfflineCarRequestValidator_InvalidFrom);
            When(dto => dto.FromUtc.HasValue(), () =>
            {
                RuleFor(dto => dto.FromUtc)
                    .InclusiveBetween(DateTime.UtcNow, DateTime.MaxValue)
                    .WithMessage(Resources.OfflineCarRequestValidator_PastFrom);
                RuleFor(dto => dto.FromUtc)
                    .LessThan(dto => dto.ToUtc)
                    .WithMessage(Resources.OfflineCarRequestValidator_FromAfterTo);
            });
            RuleFor(dto => dto.ToUtc)
                .Must(dto => dto.HasValue())
                .WithMessage(Resources.OfflineCarRequestValidator_InvalidTo);
            When(dto => dto.ToUtc.HasValue(), () =>
            {
                RuleFor(dto => dto.ToUtc)
                    .InclusiveBetween(DateTime.UtcNow, DateTime.MaxValue)
                    .WithMessage(Resources.OfflineCarRequestValidator_PastTo);
            });
        }
    }
}