using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Cars
{
    [Route("/cars/{Id}/register", "PUT")]
    public class RegisterCarRequest : PutOperation<RegisterCarResponse>
    {
        public string Id { get; set; }

        public string Jurisdiction { get; set; }

        public string Number { get; set; }
    }
}