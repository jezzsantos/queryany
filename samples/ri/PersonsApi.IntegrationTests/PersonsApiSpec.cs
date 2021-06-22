using Api.Interfaces.ServiceOperations.Persons;
using FluentAssertions;
using IntegrationTesting.Common;
using PersonsApplication.ReadModels;
using PersonsDomain;
using Storage.Interfaces;
using Xunit;

namespace PersonsApi.IntegrationTests
{
    [Trait("Category", "Integration.Web"), Collection("ThisAssembly")]
    public class PersonsApiSpec : IClassFixture<ApiSpecSetup<TestStartup>>
    {
        private readonly ApiSpecSetup<TestStartup> setup;

        public PersonsApiSpec(ApiSpecSetup<TestStartup> setup)
        {
            this.setup = setup;
            this.setup = setup;
            this.setup.Resolve<IQueryStorage<Person>>().DestroyAll();
            this.setup.Resolve<IEventStreamStorage<PersonEntity>>().DestroyAll();
        }

        [Fact]
        public void WhenCreatePerson_ThenReturnsPerson()
        {
            var person = this.setup.Api.Post(new CreatePersonRequest
            {
                FirstName = "afirstname",
                LastName = "alastname"
            }).Person;

            person.Should().NotBeNull();
        }

        [Fact]
        public void WhenGetPerson_ThenReturnsPerson()
        {
            var person = this.setup.Api.Post(new CreatePersonRequest
            {
                FirstName = "afirstname",
                LastName = "alastname"
            }).Person;

            person = this.setup.Api.Get(new GetPersonRequest
            {
                Id = person.Id
            }).Person;

            person.Should().NotBeNull();
        }
    }
}