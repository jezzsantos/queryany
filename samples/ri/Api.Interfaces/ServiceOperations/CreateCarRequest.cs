using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/cars", "POST")]
    public class CreateCarRequest : IReturn<CreateCarResponse>, IPost
    {
        public int Year { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }
    }
}