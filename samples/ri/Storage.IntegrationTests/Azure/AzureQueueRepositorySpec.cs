using System;
using Common;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Azure;
using Xunit;

namespace Storage.IntegrationTests.Azure
{
    public class AzureQueueRepositorySpecSetup : IDisposable
    {
        public AzureQueueRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Queueository = AzureQueueStorageRepository.FromSettings(NullRecorder.Instance, settings);
            AzureStorageAccountBase.InitializeAllTests();
        }

        public IQueueository Queueository { get; }

        public void Dispose()
        {
            AzureStorageAccountBase.CleanupAllTests();
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class AzureQueueRepositorySpec : AnyQueueositoryBaseSpec, IClassFixture<AzureQueueRepositorySpecSetup>
    {
        public AzureQueueRepositorySpec(AzureQueueRepositorySpecSetup setup) : base(setup.Queueository)
        {
        }
    }
}