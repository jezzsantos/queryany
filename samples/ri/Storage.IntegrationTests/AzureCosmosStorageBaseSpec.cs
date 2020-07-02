using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace Storage.IntegrationTests
{
    public abstract class AzureCosmosStorageBaseSpec : AnyStorageBaseSpec
    {
        private const string AzureCosmosDbEmulatorProcessName = @"Microsoft.Azure.Cosmos.Emulator";

        private const string AzureCosmosDbEmulatorStartupArgs =
            @"/NoExplorer /DisableRateLimiting";

        private const string AzureCosmosDbEmulatorResetArgs = @"/ResetDataPath";
        private const string AzureCosmosDbEmulatorShutdownArgs = @"/Shutdown";

        protected static void InitializeAllTests(TestContext context, string startupArguments)
        {
            EnsureAzureCosmosDbEmulatorIsStarted(startupArguments);
        }

        protected static void CleanupAllTests()
        {
            ShutdownCosmosDbEmulator();
        }

        private static void EnsureAzureCosmosDbEmulatorIsStarted(string moreStartupArguments)
        {
            ShutdownCosmosDbEmulator();

            ExecuteEmulatorCommand(AzureCosmosDbEmulatorResetArgs);

            var startupArgs = AzureCosmosDbEmulatorStartupArgs;
            if (moreStartupArguments.HasValue())
            {
                startupArgs = $"{startupArgs} {moreStartupArguments.Trim()}";
            }

            ExecuteEmulatorCommand(startupArgs, false);
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        private static void ShutdownCosmosDbEmulator()
        {
            if (IsEmulatorRunning())
            {
                ExecuteEmulatorCommand(AzureCosmosDbEmulatorShutdownArgs);
                if (IsEmulatorRunning())
                {
                    KillEmulatorProcesses();
                }
            }
        }

        private static bool IsEmulatorRunning()
        {
            return Process.GetProcesses()
                .Any(process => process.ProcessName.EqualsIgnoreCase(AzureCosmosDbEmulatorProcessName));
        }

        private static void ExecuteEmulatorCommand(string command, bool waitForCompletion = true)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = command,
                FileName = $"C:\\Program Files\\Azure Cosmos DB Emulator\\{AzureCosmosDbEmulatorProcessName}.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = true
            });
            if (waitForCompletion)
            {
                process!.WaitForExit();
            }
        }

        private static void KillEmulatorProcesses()
        {
            var processes = Process.GetProcesses()
                .Where(process => process.ProcessName.EqualsIgnoreCase(AzureCosmosDbEmulatorProcessName))
                .ToList();
            foreach (var process in processes)
            {
                try
                {
                    process.Kill(true);
                }
                catch (Exception)
                {
                    //Ignore
                }
            }
        }
    }
}