using System;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/cars/{Id}/offline", "PUT")]
    public class OfflineCarRequest : IReturn<OfflineCarResponse>, IPut
    {
        public string Id { get; set; }

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }
    }
}