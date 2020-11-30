using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Cars
{
    [Route("/cars", "POST")]
    public class CreateCarRequest : PostOperation<CreateCarResponse>
    {
        public int Year { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }
    }
}