using System.Reflection;
using Api.Common;
using Api.Common.Validators;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Funq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PersonsApplication;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Validation;
using Storage;
using Storage.Interfaces;

namespace PersonsApi
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies = {typeof(Startup).Assembly};

        public ServiceHost() : base("MyPersonsApi", AssembliesContainingServicesAndDependencies)
        {
        }

        public override void Configure(Container container)
        {
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            this.ConfigureServiceHost(debugEnabled);

            RegisterValidators(container);
            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<ILogger>(c => new Logger<ServiceHost>(new NullLoggerFactory()));
            container.AddSingleton<IDependencyContainer>(new FuncDependencyContainer(container));
            container.AddSingleton<IIdentifierFactory, GuidIdentifierFactory>();
            container.AddSingleton<IDomainFactory>(c =>
            {
                var domainFactory = new DomainFactory(c.Resolve<IDependencyContainer>());
                domainFactory.RegisterTypesFromAssemblies(typeof(PersonEntity).Assembly);
                return domainFactory;
            });
            container.AddSingleton<IPersonStorage>(c => new PersonStorage(c.Resolve<IStorage<PersonEntity>>()));
            container.AddSingleton<IStorage<PersonEntity>>(c =>
                PersonEntityAzureStorage.Create(c.Resolve<ILogger>(), c.Resolve<IAppSettings>(),
                    c.Resolve<IDomainFactory>()));
            container.AddSingleton<IPersonsApplication, PersonsApplication.PersonsApplication>();
        }

        private void RegisterValidators(Container container)
        {
            Plugins.Add(new ValidationFeature());
            container.RegisterValidators(AssembliesContainingServicesAndDependencies);
            container.AddSingleton<IHasSearchOptionsValidator, HasSearchOptionsValidator>();
            container.AddSingleton<IHasGetOptionsValidator, HasGetOptionsValidator>();
        }
    }
}