using System.Reflection;
using Api.Common;
using Api.Common.Validators;
using ApplicationServices;
using CarsApplication;
using CarsApplication.Storage;
using CarsDomain;
using CarsStorage;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Funq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceClients;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Validation;
using Storage.Azure;

namespace CarsApi
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies = {typeof(Startup).Assembly};

        public ServiceHost() : base("MyCarsApi", AssembliesContainingServicesAndDependencies)
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
            container.AddSingleton<IIdentifierFactory, CarIdentifierFactory>();
            container.AddSingleton<IDomainFactory>(c =>
                DomainFactory.CreateRegistered(c.Resolve<IDependencyContainer>(), typeof(EntityEvent).Assembly,
                    typeof(CarEntity).Assembly));
            container.AddSingleton<ICarStorage>(c =>
                new CarStorage(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(),
                    AzureCosmosSqlApiRepository.FromAppSettings(c.Resolve<IAppSettings>(), "Production")));
            container.AddSingleton<ICarsApplication, CarsApplication.CarsApplication>();
            container.AddSingleton<IPersonsService>(c =>
                new PersonsServiceClient(c.Resolve<IAppSettings>().GetString("PersonsApiBaseUrl")));
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