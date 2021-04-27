namespace Api.Interfaces.ServiceOperations.Health
{
    public class GetHealthCheckResponse
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string Status { get; set; } = "OK";
    }
}