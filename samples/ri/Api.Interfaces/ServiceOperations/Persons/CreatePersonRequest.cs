using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Persons
{
    [Route("/persons", "POST")]
    public class CreatePersonRequest : PostOperation<CreatePersonResponse>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}