using System.Reflection;
using Api.Common;
using Api.Common.Validators;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
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
using Storage.Azure;

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
            container.AddSingleton<IIdentifierFactory, PersonIdentifierFactory>();
            container.AddSingleton<IDomainFactory>(c =>
                DomainFactory.CreateRegistered(c.Resolve<IDependencyContainer>(), typeof(EntityEvent).Assembly,
                    typeof(PersonEntity).Assembly));
            container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(),
                    AzureCosmosSqlApiRepository.FromAppSettings(c.Resolve<IAppSettings>(), "Production")));
            container.AddSingleton<IPersonsApplication, PersonsApplication.PersonsApplication>();
            container.AddSingleton<IEmailService, EmailService>();
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