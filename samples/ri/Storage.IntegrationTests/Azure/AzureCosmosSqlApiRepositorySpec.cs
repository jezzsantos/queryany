using System;
using Common;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Azure;
using Xunit;

namespace Storage.IntegrationTests.Azure
{
    public class AzureCosmosSqlApiRepositorySpecSetup : IDisposable
    {
        public AzureCosmosSqlApiRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Repository = AzureCosmosSqlApiRepository.FromSettings(NullRecorder.Instance, settings, "TestDatabase");
            AzureCosmosStorageBase.InitializeAllTests(null);
        }

        public IRepository Repository { get; }

        public void Dispose()
        {
            AzureCosmosStorageBase.CleanupAllTests();
        }
    }

    [Trait("Category", "Integration.Storage.NOCI")]
    public class AzureCosmosSqlApiRepositorySpec : AnyRepositoryBaseSpec,
        IClassFixture<AzureCosmosSqlApiRepositorySpecSetup>
    {
        public AzureCosmosSqlApiRepositorySpec(AzureCosmosSqlApiRepositorySpecSetup setup) : base(setup.Repository)
        {
        }
    }
}