using System;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/cars/{Id}/occupy", "PUT")]
    public class OccupyCarRequest : IReturn<OccupyCarResponse>, IPut
    {
        public string Id { get; set; }

        public DateTime UntilUtc { get; set; }
    }
}