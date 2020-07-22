using ServiceStack;

namespace Services.Interfaces.ServiceOperations
{
    [Route("/cars/available", "GET")]
    public class SearchAvailableCarsRequest : SearchOperation<SearchAvailableCarsResponse>
    {
        // TODO: Add any specific search filters eg. for location, or calendar
    }
}