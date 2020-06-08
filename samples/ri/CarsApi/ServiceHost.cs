using System.Reflection;
using CarsApi.Storage;
using CarsApi.Validators;
using CarsDomain;
using CarsDomain.Entities;
using Funq;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.Validation;
using Storage.Interfaces;

namespace CarsApi
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] assembliesContainingServicesAndDependencies = new Assembly[] { typeof(Startup).Assembly };

        public ServiceHost() : base("MyCarsApi", assembliesContainingServicesAndDependencies) { }

        public override void Configure(Container container)
        {
            SetConfig(new HostConfig
            {
                DefaultRedirectPath = "/metadata",
                DebugMode = AppSettings.Get(nameof(HostConfig.DebugMode), false),
            });

            RegisterValidators(container);
            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IStorage<CarEntity>, CarInMemStorage>();
            container.AddSingleton<Cars>();
        }

        private void RegisterValidators(Container container)
        {
            Plugins.Add(new ValidationFeature());
            container.RegisterValidators(assembliesContainingServicesAndDependencies);
            container.AddSingleton<IHasSearchOptionsValidator, Validators.HasSearchOptions>();
            container.AddSingleton<IHasGetOptionsValidator, Validators.HasGetOptions>();
        }
    }
}
