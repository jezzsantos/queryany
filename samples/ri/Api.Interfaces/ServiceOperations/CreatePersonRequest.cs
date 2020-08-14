using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/persons", "POST")]
    public class CreatePersonRequest : IReturn<CreatePersonResponse>, IPost
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}