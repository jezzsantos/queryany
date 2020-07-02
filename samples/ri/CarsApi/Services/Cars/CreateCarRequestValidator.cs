using System;
using System.Linq;
using CarsApi.Properties;
using QueryAny.Primitives;
using Services.Interfaces.Apis;
using ServiceStack.FluentValidation;

namespace CarsApi.Services.Cars
{
    internal class CreateCarRequestValidator : AbstractValidator<CreateCarRequest>
    {
        internal const int MinYear = 1917;
        internal readonly string[] Makes = {"Honda", "Toyota"};
        internal readonly int MaxYear = DateTime.UtcNow.AddYears(3).Year;
        internal readonly string[] Models = {"Civic", "Surf"};

        public CreateCarRequestValidator()
        {
            RuleFor(dto => dto.Year).InclusiveBetween(MinYear, this.MaxYear)
                .WithMessage(Resources.CreateCarRequestValidator_InvalidYear.Format(MinYear, this.MaxYear));
            RuleFor(dto => dto.Make).Must(val => this.Makes.Contains(val))
                .WithMessage(Resources.CreateCarRequestValidator_InvalidMake.Format(string.Join(", ", this.Makes)));
            RuleFor(dto => dto.Model).Must(val => this.Models.Contains(val))
                .WithMessage(Resources.CreateCarRequestValidator_InvalidModel.Format(string.Join(", ", this.Models)));
        }
    }
}