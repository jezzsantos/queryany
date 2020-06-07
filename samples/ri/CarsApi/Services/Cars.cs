using Services.Interfaces;
using Services.Interfaces.Apis;
using ServiceStack;

namespace CarsApi.Services
{
    public class Cars : Service
    {
        public CarsDomain.Cars CarsService { get; set; }

        public GetAvailableCarsResponse Get(GetAvailableCars request)
        {
            return new GetAvailableCarsResponse
            {
                Cars = CarsService.SearchAvailable(new SearchOptions(), new GetOptions())
            };
        }
    }
}
