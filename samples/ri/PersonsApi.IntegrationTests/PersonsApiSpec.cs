using Api.Interfaces.ServiceOperations.Persons;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonsApplication.ReadModels;
using PersonsDomain;
using ServiceStack;
using Storage.Interfaces;

namespace PersonsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration.Web")]
    public class PersonsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private static IWebHost webHost;
        private static IEventStreamStorage<PersonEntity> personEventingStorage;
        private static IQueryStorage<Person> personQueryStorage;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            webHost = WebHost.CreateDefaultBuilder(null)
                .UseModularStartup<TestStartup>()
                .UseUrls(ServiceUrl)
                .UseKestrel()
                .ConfigureLogging((ctx, builder) => builder.AddConsole())
                .Build();
            webHost.Start();

            var container = HostContext.Container;
            personQueryStorage = container.Resolve<IQueryStorage<Person>>();
            personEventingStorage = container.Resolve<IEventStreamStorage<PersonEntity>>();
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void Initialize()
        {
            personEventingStorage.DestroyAll();
            personQueryStorage.DestroyAll();
        }

        [TestMethod]
        public void WhenCreatePerson_ThenReturnsPerson()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var person = client.Post(new CreatePersonRequest
            {
                FirstName = "afirstname",
                LastName = "alastname"
            }).Person;

            person.Should().NotBeNull();
        }

        [TestMethod]
        public void WhenGetPerson_ThenReturnsPerson()
        {
            var client = new JsonServiceClient(ServiceUrl);

            var person = client.Post(new CreatePersonRequest
            {
                FirstName = "afirstname",
                LastName = "alastname"
            }).Person;

            person = client.Get(new GetPersonRequest
            {
                Id = person.Id
            }).Person;

            person.Should().NotBeNull();
        }
    }
}