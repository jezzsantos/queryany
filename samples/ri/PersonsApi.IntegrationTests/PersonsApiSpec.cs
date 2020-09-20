using Api.Interfaces.ServiceOperations;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonsApplication.ReadModels;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using Storage;
using Storage.Interfaces;
using IRepository = Storage.IRepository;

namespace PersonsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration.Web")]
    public class PersonsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private static IWebHost webHost;
        private static IEventStreamStorage<PersonEntity> eventingStorage;
        private static IQueryStorage<Person> queryStorage;
        private static IRepository inMemRepository;

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
            inMemRepository = new InProcessInMemRepository();
            eventingStorage = new GeneralEventStreamStorage<PersonEntity>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);
            queryStorage = new GeneralQueryStorage<Person>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), inMemRepository);
            container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(eventingStorage, queryStorage));
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            webHost?.StopAsync().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void Initialize()
        {
            eventingStorage.DestroyAll();
            queryStorage.DestroyAll();
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