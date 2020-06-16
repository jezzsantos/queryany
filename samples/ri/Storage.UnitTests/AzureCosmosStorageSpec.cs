using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass]
    public class AzureCosmosStorageSpec : AnyStorageBaseSpec
    {
        private const string AzureCosmosDbEmulatorProcessName = @"Microsoft.Azure.Cosmos.Emulator";

        private const string AzureCosmosDbEmulatorStartupArgs =
            @"/EnableTableEndpoint /NoExplorer /DisableRateLimiting";

        private const string AzureCosmosDbEmulatorShutdownArgs = @"/Shutdown";

        [ClassInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            EnsureAzureCosmosDbEmulatorIsStarted();
        }

        [ClassCleanup]
        public static void CleanupAllTests()
        {
            ShutdownCosmosDbEmulator();
        }

        private static void EnsureAzureCosmosDbEmulatorIsStarted()
        {
            if (!Process.GetProcesses()
                .Any(process => process.ProcessName.EqualsIgnoreCase(AzureCosmosDbEmulatorProcessName)))
            {
                Process.Start(new ProcessStartInfo
                {
                    Arguments = AzureCosmosDbEmulatorStartupArgs,
                    FileName = $"C:\\Program Files\\Azure Cosmos DB Emulator\\{AzureCosmosDbEmulatorProcessName}.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    UseShellExecute = false
                });
            }
        }

        private static void ShutdownCosmosDbEmulator()
        {
            if (Process.GetProcesses()
                .Any(process => process.ProcessName.EqualsIgnoreCase(AzureCosmosDbEmulatorProcessName)))
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    Arguments = AzureCosmosDbEmulatorShutdownArgs,
                    FileName = $"C:\\Program Files\\Azure Cosmos DB Emulator\\{AzureCosmosDbEmulatorProcessName}.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas",
                    UseShellExecute = true
                });
                proc!.WaitForExit();
            }
        }

        protected override IStorage<TestEntity> GetStorage()
        {
            var config = new ConfigurationBuilder().AddJsonFile(@"appsettings.json").Build();
            var connectionString = config["AzureCosmosDbConnectionString"];
            return new TestAzureCosmosStorage(
                new AzureCosmosConnection(new AzureCosmosRepository(connectionString, new GuidIdentifierFactory())));
        }
    }

    public class TestAzureCosmosStorage : AzureCosmosStorage<TestEntity>
    {
        public TestAzureCosmosStorage(IAzureCosmosConnection connection) : base(connection)
        {
        }

        protected override string ContainerName => "TestEntities";
    }
}