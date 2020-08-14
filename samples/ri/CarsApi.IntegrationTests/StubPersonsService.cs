using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using ServiceClients;

namespace CarsApi.IntegrationTests
{
    public class StubPersonsService : IPersonsService
    {
        public Person Get(Identifier id)
        {
            return new Person
            {
                Id = id.ToString(),
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                }
            };
        }
    }
}