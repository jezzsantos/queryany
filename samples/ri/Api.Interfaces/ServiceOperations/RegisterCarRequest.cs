using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/cars/{Id}/register", "PUT")]
    public class RegisterCarRequest : IReturn<RegisterCarResponse>, IPut
    {
        public string Id { get; set; }

        public string Jurisdiction { get; set; }

        public string Number { get; set; }
    }
}