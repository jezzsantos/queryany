using CarsApi.Validators;
using QueryAny.Primitives;
using Services.Interfaces.Apis;
using ServiceStack.FluentValidation;

namespace CarsApi.Services.Cars
{
    internal class SearchAvailableCarsRequestValidator : AbstractValidator<SearchAvailableCarsRequest>
    {
        public SearchAvailableCarsRequestValidator(IHasSearchOptionsValidator hasSearchOptionsValidator)
        {
            Guard.AgainstNull(() => hasSearchOptionsValidator, hasSearchOptionsValidator);

            Include(hasSearchOptionsValidator);
        }
    }
}