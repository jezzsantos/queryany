using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Azure
{
    // ReSharper disable once InconsistentNaming
    public static class AzureCosmosStorageBase
    {
        private const string EmulatorProcessName = @"Microsoft.Azure.Cosmos.Emulator";
        private const string EmulatorStartupArgs = @"/NoExplorer /DisableRateLimiting";
        private const string EmulatorResetArgs = @"/ResetDataPath";
        private const string EmulatorShutdownArgs = @"/Shutdown";

        public static void InitializeAllTests(TestContext context, string startupArguments)
        {
            EnsureAzureCosmosDbEmulatorIsStarted(startupArguments);
        }

        public static void CleanupAllTests()
        {
            ShutdownCosmosDbEmulator();
        }

        private static void EnsureAzureCosmosDbEmulatorIsStarted(string moreStartupArguments)
        {
            ShutdownCosmosDbEmulator();

            ExecuteEmulatorCommand(EmulatorResetArgs);

            var startupArgs = EmulatorStartupArgs;
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
                ExecuteEmulatorCommand(EmulatorShutdownArgs);
                if (IsEmulatorRunning())
                {
                    KillEmulatorProcesses();
                }
            }
        }

        private static bool IsEmulatorRunning()
        {
            return Process.GetProcesses()
                .Any(process => process.ProcessName.EqualsIgnoreCase(EmulatorProcessName));
        }

        private static void ExecuteEmulatorCommand(string command, bool waitForCompletion = true)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = command,
                FileName = $"C:\\Program Files\\Azure Cosmos DB Emulator\\{EmulatorProcessName}.exe",
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
                .Where(process => process.ProcessName.EqualsIgnoreCase(EmulatorProcessName))
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