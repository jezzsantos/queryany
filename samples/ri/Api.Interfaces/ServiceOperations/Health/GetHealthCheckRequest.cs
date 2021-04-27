using ServiceStack;

namespace Api.Interfaces.ServiceOperations.Health
{
    [Route("/health", "GET")]
    public class GetHealthCheckRequest : GetOperation<GetHealthCheckResponse>
    {
    }
}