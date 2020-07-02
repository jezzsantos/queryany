using System.Reflection;
using CarsApi.Validators;
using CarsDomain;
using CarsDomain.Entities;
using Funq;
using ServiceStack;
using ServiceStack.Validation;
using Storage;
using Storage.Interfaces;

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
            SetConfig(new HostConfig
            {
                DefaultRedirectPath = "/metadata",
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), false)
            });

            RegisterValidators(container);
            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IIdentifierFactory, GuidIdentifierFactory>();
            container.AddSingleton(c => new InProcessInMemRepository(c.Resolve<IIdentifierFactory>()));
            container.AddSingleton<IStorage<CarEntity>>(c =>
                new CarInMemStorage(c.Resolve<InProcessInMemRepository>()));
            container.AddSingleton<ICars, Cars>();
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