using System;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Xunit;

namespace Storage.IntegrationTests
{
    public class LocalMachineFileRepositorySpecSetup : IDisposable
    {
        public LocalMachineFileRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Repository = LocalMachineFileRepository.FromSettings(settings);
            Blobository = LocalMachineFileRepository.FromSettings(settings);
        }

        public IRepository Repository { get; }

        public IBlobository Blobository { get; }

        public void Dispose()
        {
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class LocalMachineFileRepositorySpec : AnyRepositoryBaseSpec,
        IClassFixture<LocalMachineFileRepositorySpecSetup>
    {
        public LocalMachineFileRepositorySpec(LocalMachineFileRepositorySpecSetup setup) : base(setup.Repository)
        {
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class LocalMachineFileBlobositorySpec : AnyBlobositoryBaseSpec,
        IClassFixture<LocalMachineFileRepositorySpecSetup>
    {
        public LocalMachineFileBlobositorySpec(LocalMachineFileRepositorySpecSetup setup) : base(setup.Blobository)
        {
        }
    }
}