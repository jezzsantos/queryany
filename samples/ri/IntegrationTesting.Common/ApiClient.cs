using ServiceStack;

namespace IntegrationTesting.Common
{
    public static class ApiClient
    {
        public static JsonServiceClient Create(string serviceUrl)
        {
            return new JsonServiceClient($"{serviceUrl.WithTrailingSlash()}");
        }
    }
}