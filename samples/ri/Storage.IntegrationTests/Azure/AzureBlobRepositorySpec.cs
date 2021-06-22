using System;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Azure;
using Xunit;

namespace Storage.IntegrationTests.Azure
{
    public class AzureBlobRepositorySpecSetup : IDisposable
    {
        public AzureBlobRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Blobository = AzureBlobStorageRepository.FromSettings(settings);
            AzureStorageAccountBase.InitializeAllTests();
        }

        public IBlobository Blobository { get; }

        public void Dispose()
        {
            AzureStorageAccountBase.CleanupAllTests();
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class AzureBlobRepositorySpec : AnyBlobositoryBaseSpec, IClassFixture<AzureBlobRepositorySpecSetup>
    {
        public AzureBlobRepositorySpec(AzureBlobRepositorySpecSetup setup) : base(setup.Blobository)
        {
        }
    }
}