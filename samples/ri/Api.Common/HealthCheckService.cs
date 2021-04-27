using Api.Interfaces.ServiceOperations.Health;
using ServiceStack;

namespace Api.Common
{
    public class HealthCheckService : Service
    {
        public GetHealthCheckResponse Get(GetHealthCheckRequest request)
        {
            return new GetHealthCheckResponse
            {
                Name = HostContext.ServiceName,
                Version = HostContext.AppHost.GetAssemblyFileVersion()
            };
        }
    }
}