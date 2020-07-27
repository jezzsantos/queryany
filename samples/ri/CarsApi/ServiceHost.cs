using System.Reflection;
using CarsApi.Validators;
using CarsDomain;
using CarsDomain.Entities;
using CarsStorage;
using Funq;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Text;
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
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            SetupServiceConfig(debugEnabled);

            RegisterValidators(container);
            RegisterDependencies(container);
        }

        private void SetupServiceConfig(bool debugEnabled)
        {
            SetConfig(new HostConfig
            {
                DebugMode = debugEnabled,
                DefaultRedirectPath = "/metadata"
            });

            SetupJsonResponses();
        }

        private static void SetupJsonResponses()
        {
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.AssumeUtc = true;
            JsConfig.AlwaysUseUtc = true;
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IStorage<CarEntity>>(c =>
                CarEntityAzureStorage.Create(c.Resolve<ILogger>(), container.Resolve<IAppSettings>(),
                    new GuidIdentifierFactory()));
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