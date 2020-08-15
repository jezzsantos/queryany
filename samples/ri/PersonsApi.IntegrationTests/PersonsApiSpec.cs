using Api.Interfaces.ServiceOperations;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using Storage.Interfaces;

namespace PersonsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration")]
    public class PersonsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private static IWebHost webHost;
        private static IStorage<PersonEntity> storage;

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            webHost = WebHost.CreateDefaultBuilder(null)
                .UseModularStartup<Startup>()
                .UseUrls(ServiceUrl)
                .UseKestrel()
                .ConfigureLogging((ctx, builder) => builder.AddConsole())
                .Build();
            webHost.Start();

            // Override services for testing
            var container = HostContext.Container;
            container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(c.Resolve<IStorage<PersonEntity>>()));
            storage = PersonEntityInMemStorage.Create(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>());
            container.AddSingleton(storage);

            container.AddSingleton<IStorage<PersonEntity>>(c =>
                PersonEntityInMemStorage.Create(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>()));
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
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