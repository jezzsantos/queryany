using Api.Interfaces.ServiceOperations;
using Domain.Interfaces.Entities;
using FluentAssertions;
using InfrastructureServices.Eventing.ReadModels;
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
using Storage.ReadModels;
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
        private static IRepository repository;

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
            repository = new InProcessInMemRepository();

            queryStorage = new GeneralQueryStorage<Person>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(), repository);
            eventingStorage = new GeneralEventStreamStorage<PersonEntity>(container.Resolve<ILogger>(),
                container.Resolve<IDomainFactory>(),
                container.Resolve<IChangeEventMigrator>(), repository);

            container.AddSingleton(eventingStorage);
            container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(eventingStorage, queryStorage));
            container.AddSingleton<IReadModelProjectionSubscription>(c => new InProcessReadModelProjectionSubscription(
                c.Resolve<ILogger>(),
                new ReadModelProjector(c.Resolve<ILogger>(),
                    new ReadModelCheckpointStore(c.Resolve<ILogger>(), c.Resolve<IIdentifierFactory>(),
                        c.Resolve<IDomainFactory>(), repository),
                    c.Resolve<IChangeEventMigrator>(),
                    new PersonEntityReadModelProjection(c.Resolve<ILogger>(), repository)),
                c.Resolve<IEventStreamStorage<PersonEntity>>()));

            //HACK: subscribe again (see: https://forums.servicestack.net/t/integration-testing-and-overriding-registered-services/8875/5)
            HostContext.AppHost.OnAfterInit();
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