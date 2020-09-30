using Application.Resources;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/persons/{Id}", "GET")]
    public class GetPersonRequest : GetOperation<GetPersonResponse>
    {
        public string Id { get; set; }
    }

    public class GetPersonResponse
    {
        public Person Person { get; set; }
    }
}