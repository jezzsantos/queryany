using System;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Cars
{
    [Route("/cars/available", "GET")]
    public class SearchAvailableCarsRequest : SearchOperation<SearchAvailableCarsResponse>
    {
        public DateTime? FromUtc { get; set; }

        public DateTime? ToUtc { get; set; }
    }
}