using Services.Interfaces;
using Services.Interfaces.Apis;
using ServiceStack;

namespace CarsApi.Services.Cars
{
    public class CarsService : Service
    {
        public CarsDomain.Cars Cars { get; set; }

        public SearchAvailableCarsResponse Get(SearchAvailableCarsRequest request)
        {
            return new SearchAvailableCarsResponse
            {
                Cars = Cars.SearchAvailable(request.ToSearchOptions(), request.ToGetOptions())
            };
        }
    }
}
