using System.Reflection;
using CarsApi.Validators;
using CarsDomain;
using CarsDomain.Entities;
using Funq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Validation;
using Storage;
using Storage.Azure;
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
            RegisterCarsStorage(container);
            container.AddSingleton<ICars, Cars>();
        }

        private void RegisterValidators(Container container)
        {
            Plugins.Add(new ValidationFeature());
            container.RegisterValidators(AssembliesContainingServicesAndDependencies);
            container.AddSingleton<IHasSearchOptionsValidator, HasSearchOptionsValidator>();
            container.AddSingleton<IHasGetOptionsValidator, HasGetOptionsValidator>();
        }

        private static void RegisterCarsStorage(Container container)
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var accountKey = config["AzureCosmosDbAccountKey"];
            var hostName = config["AzureCosmosDbHostName"];
            var localEmulatorConnectionString = $"AccountEndpoint=https://{hostName}:8081/;AccountKey={accountKey}";
            container.AddSingleton<IStorage<CarEntity>>(c =>
                new CarEntityAzureStorage(c.Resolve<ILogger>(), new AzureStorageConnection(
                    new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "Production",
                        new GuidIdentifierFactory()))));
        }
    }
}