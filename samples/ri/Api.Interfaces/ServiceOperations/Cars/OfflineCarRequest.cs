using System;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Cars
{
    [Route("/cars/{Id}/offline", "PUT")]
    public class OfflineCarRequest : PutOperation<OfflineCarResponse>
    {
        public string Id { get; set; }

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }
    }
}