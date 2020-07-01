using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Apis;
using ServiceStack;

namespace CarsApi.Services.Cars
{
    public class CarsService : Service
    {
        private readonly CarsDomain.Cars cars;

        public CarsService(CarsDomain.Cars cars)
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
    }
}