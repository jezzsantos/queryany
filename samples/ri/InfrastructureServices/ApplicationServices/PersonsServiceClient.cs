using Api.Interfaces.ServiceOperations.Persons;
using Application.Resources;
using ApplicationServices.Interfaces;
using Common;
using ServiceStack;

namespace InfrastructureServices.ApplicationServices
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