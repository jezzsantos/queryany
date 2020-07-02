using CarsDomain;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Apis;
using ServiceStack;

namespace CarsApi.Services.Cars
{
    public class CarsService : Service
    {
        private readonly ICars cars;

        public CarsService(ICars cars)
        {
            Guard.AgainstNull(() => cars, cars);

            this.cars = cars;
        }

        public SearchAvailableCarsResponse Get(SearchAvailableCarsRequest request)
        {
            return new SearchAvailableCarsResponse
            {
                Cars = this.cars.SearchAvailable(request.ToSearchOptions(), request.ToGetOptions())
            };
        }

        public CreateCarResponse Post(CreateCarRequest request)
        {
            return new CreateCarResponse
            {
                Car = this.cars.Create(request.Year, request.Make, request.Model)
            };
        }
    }
}