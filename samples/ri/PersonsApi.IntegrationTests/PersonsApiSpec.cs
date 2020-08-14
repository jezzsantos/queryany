using Api.Common;
using Api.Interfaces.ServiceOperations;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using Storage;
using Storage.Interfaces;

namespace PersonsApi.IntegrationTests
{
    [TestClass, TestCategory("Integration")]
    public class PersonsApiSpec
    {
        private const string ServiceUrl = "http://localhost:2000/";
        private ServiceStackHost appHost;
        private IDomainFactory domainFactory;
        private ILogger logger;
        private PersonEntityInMemStorage store;

        [TestInitialize]
        public void Initialize()
        {
            this.appHost = new TestServiceHost();
            this.logger = new Logger<TestServiceHost>(new NullLoggerFactory());
            this.domainFactory = new DomainFactory(new FuncDependencyContainer(this.appHost.Container));
            this.domainFactory.RegisterTypesFromAssemblies(typeof(PersonEntity).Assembly);
            this.store = PersonEntityInMemStorage.Create(this.logger, this.domainFactory);
            this.appHost.Container.AddSingleton<IIdentifierFactory, GuidIdentifierFactory>();
            this.appHost.Container.AddSingleton(this.logger);
            this.appHost.Container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(c.Resolve<IStorage<PersonEntity>>()));
            this.appHost.Container.AddSingleton<IStorage<PersonEntity>>(this.store);
            this.appHost.Init()
                .Start(ServiceUrl);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.appHost.Dispose();
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