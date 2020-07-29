using CarsDomain;
using QueryAny.Primitives;
using Services.Interfaces;
using Services.Interfaces.Resources;
using Services.Interfaces.ServiceOperations;
using ServiceStack;

namespace CarsApi.Services.Cars
{
    internal class CarsService : Service
    {
        private readonly ICars cars;

        public CarsService(ICars cars)
        {
            cars.GuardAgainstNull(nameof(cars));

            this.cars = cars;
        }

        public SearchAvailableCarsResponse Get(SearchAvailableCarsRequest request)
        {
            var available = this.cars.SearchAvailable(Request.ToCaller(),
                request.ToSearchOptions(defaultSort: Reflector<Car>.GetPropertyName(c => c.OccupiedUntilUtc)),
                request.ToGetOptions());
            return new SearchAvailableCarsResponse
            {
                Cars = available.Results,
                Metadata = available.Metadata
            };
        }

        public CreateCarResponse Post(CreateCarRequest request)
        {
            return new CreateCarResponse
            {
                Car = this.cars.Create(Request.ToCaller(), request.Year, request.Make, request.Model)
            };
        }

        public OccupyCarResponse Put(OccupyCarRequest request)
        {
            return new OccupyCarResponse
            {
                Car = this.cars.Occupy(Request.ToCaller(), request.Id, request.UntilUtc)
            };
        }
    }
}