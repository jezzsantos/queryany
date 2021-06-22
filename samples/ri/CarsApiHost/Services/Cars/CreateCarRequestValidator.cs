using Api.Interfaces.ServiceOperations.Cars;
using CarsApiHost.Properties;
using CarsDomain;
using Common;
using ServiceStack.FluentValidation;

namespace CarsApiHost.Services.Cars
{
    internal class CreateCarRequestValidator : AbstractValidator<CreateCarRequest>
    {
        public CreateCarRequestValidator()
        {
            RuleFor(dto => dto.Year)
                .InclusiveBetween(Manufacturer.MinYear, Manufacturer.MaxYear)
                .WithMessage(
                    Resources.CreateCarRequestValidator_InvalidYear.Format(Manufacturer.MinYear, Manufacturer.MaxYear));
            RuleFor(dto => dto.Make)
                .NotEmpty();
            When(dto => dto.Make.HasValue(), () =>
            {
                RuleFor(dto => dto.Make)
                    .Must(val => Manufacturer.Makes.Contains(val))
                    .WithMessage(
                        Resources.CreateCarRequestValidator_InvalidMake.Format(string.Join(", ", Manufacturer.Makes)));
            });
            RuleFor(dto => dto.Model)
                .NotEmpty();
            When(dto => dto.Model.HasValue(), () =>
            {
                RuleFor(dto => dto.Model)
                    .NotEmpty()
                    .Must(val => Manufacturer.Models.Contains(val))
                    .WithMessage(
                        Resources.CreateCarRequestValidator_InvalidModel.Format(string.Join(", ",
                            Manufacturer.Models)));
            });
        }
    }
}