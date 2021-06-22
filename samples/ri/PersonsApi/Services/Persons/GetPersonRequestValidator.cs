﻿using Api.Common.Validators;
using Api.Interfaces.ServiceOperations.Persons;
using Common;
using Domain.Interfaces.Entities;
using PersonsApi.Properties;
using ServiceStack.FluentValidation;

namespace PersonsApi.Services.Persons
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