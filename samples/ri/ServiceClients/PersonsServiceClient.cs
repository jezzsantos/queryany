using Api.Interfaces.ServiceOperations;
using Application.Resources;
using ApplicationServices;
using QueryAny.Primitives;
using ServiceStack;

namespace ServiceClients
{
    public class PersonsServiceClient : IPersonsService
    {
        private readonly string baseUrl;

        public PersonsServiceClient(string serviceBaseUrl)
        {
            serviceBaseUrl.GuardAgainstNullOrEmpty(nameof(serviceBaseUrl));
            this.baseUrl = serviceBaseUrl;
        }

        public Person Get(string id)
        {
            var client = new JsonServiceClient(this.baseUrl);

            return client.Get(new GetPersonRequest {Id = id}).Person;
        }
    }
}