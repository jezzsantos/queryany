using System;
using Common;
using Microsoft.Extensions.Configuration;
using ServiceStack;
using Storage.Sql;
using Xunit;

namespace Storage.IntegrationTests.Sql
{
    public class SqlServerRepositorySpecSetup : IDisposable
    {
        public SqlServerRepositorySpecSetup()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("Storage:SqlServerServiceName");
            Repository = SqlServerRepository.FromSettings(NullRecorder.Instance, settings);
            SqlServerStorageBase.InitializeAllTests(serviceName, "TestDatabase");
        }

        public IRepository Repository { get; }

        public void Dispose()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var settings = new NetCoreAppSettings(config);
            var serviceName = settings.GetString("Storage:SqlServerServiceName");
            SqlServerStorageBase.CleanupAllTests(serviceName);
        }
    }

    [Trait("Category", "Integration.Storage")]
    public class SqlServerRepositorySpec : AnyRepositoryBaseSpec, IClassFixture<SqlServerRepositorySpecSetup>
    {
        public SqlServerRepositorySpec(SqlServerRepositorySpecSetup setup) : base(setup.Repository)
        {
        }
    }
}