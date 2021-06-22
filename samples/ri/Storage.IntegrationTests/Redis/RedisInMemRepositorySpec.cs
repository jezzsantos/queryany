using System;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Redis;
using Xunit;

namespace Storage.IntegrationTests.Redis
{
    public class RedisInMemRepositorySpecSetup : IDisposable
    {
        public RedisInMemRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            Repository = RedisInMemRepository.FromSettings(settings);
            RedisInMemStorageBase.InitializeAllTests();
        }

        public IRepository Repository { get; }

        public void Dispose()
        {
            RedisInMemStorageBase.CleanupAllTests();
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class RedisInMemRepositorySpec : AnyRepositoryBaseSpec, IClassFixture<RedisInMemRepositorySpecSetup>
    {
        public RedisInMemRepositorySpec(RedisInMemRepositorySpecSetup setup) : base(setup.Repository)
        {
        }
    }
}