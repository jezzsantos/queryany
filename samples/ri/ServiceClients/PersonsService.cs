using Api.Interfaces.ServiceOperations;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using QueryAny.Primitives;
using ServiceStack;

namespace ServiceClients
{
    public class PersonsService : IPersonsService
    {
        private readonly string baseUrl;

        public PersonsService(string serviceBaseUrl)
        {
            serviceBaseUrl.GuardAgainstNullOrEmpty(nameof(serviceBaseUrl));
            this.baseUrl = serviceBaseUrl;
        }

        public Person Get(Identifier id)
        {
            var client = new JsonServiceClient(this.baseUrl);

            return client.Get(new GetPersonRequest {Id = id.ToString()}).Person;
        }
    }
}