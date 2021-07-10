using System;
using Common;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Azure;
using Storage.Azure.Properties;
using UnitTesting.Common;
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
        private readonly AzureQueueRepositorySpecSetup setup;

        public AzureQueueRepositorySpec(AzureQueueRepositorySpecSetup setup) : base(setup.Queueository)
        {
            this.setup = setup;
        }

        [Fact]
        public void WhenPushWithInvalidQueueName_ThenThrows()
        {
            this.setup.Queueository
                .Invoking(x => x.Push("^aninvalidqueuename^", "amessage"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenPopSingleWithInvalidQueueName_ThenThrows()
        {
            this.setup.Queueository
                .Invoking(x => x.PopSingle("^aninvalidqueuename^", s => { }))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenCountWithInvalidQueueName_ThenThrows()
        {
            this.setup.Queueository
                .Invoking(x => x.Count("^aninvalidqueuename^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }

        [Fact]
        public void WhenDestroyAllWithInvalidQueueName_ThenThrows()
        {
            this.setup.Queueository
                .Invoking(x => x.DestroyAll("^aninvalidqueuename^"))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessageLike(Resources.AzureQueueStorageRepository_InvalidStorageName);
        }
    }
}