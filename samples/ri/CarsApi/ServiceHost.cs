using System.Reflection;
using CarsApi.Validators;
using CarsDomain;
using CarsDomain.Entities;
using Funq;
using Microsoft.Extensions.Configuration;
using ServiceStack;
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
                new CarEntityAzureStorage(new AzureStorageConnection(
                    new AzureCosmosSqlApiRepository(localEmulatorConnectionString, "Production",
                        new GuidIdentifierFactory()))));
        }
    }
}