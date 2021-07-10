using System;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Azure;
using Storage.Azure.Properties;
using UnitTesting.Common;
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
        private readonly AzureBlobRepositorySpecSetup setup;

        public AzureBlobRepositorySpec(AzureBlobRepositorySpecSetup setup) : base(setup.Blobository)
        {
            this.setup = setup;
        }

        [Fact]
        public void WhenDownloadWithInvalidContainerName_ThenThrows()
        {
            this.setup.Blobository
                .Invoking(x => x.Download("^aninvalidcontainername^", "ablobname", Stream.Null))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenUploadWithInvalidContainerName_ThenThrows()
        {
            this.setup.Blobository
                .Invoking(x => x.Upload("^aninvalidcontainername^", "ablobname", "aconttenttype", new byte[0]))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenDestroyAllWithInvalidContainerName_ThenThrows()
        {
            this.setup.Blobository
                .Invoking(x => x.DestroyAll("^aninvalidcontainername^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }
    }
}