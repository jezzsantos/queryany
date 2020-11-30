using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Persons
{
    [Route("/persons/{Id}", "GET")]
    public class GetPersonRequest : GetOperation<GetPersonResponse>
    {
        public string Id { get; set; }
    }
}