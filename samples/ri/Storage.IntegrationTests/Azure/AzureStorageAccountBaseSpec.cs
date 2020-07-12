using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Azure
{
    public abstract class AzureStorageAccountBaseSpec : AnyStorageBaseSpec
    {
        private const string AzureStorageEmulatorProcessName = @"AzureStorageEmulator";
        private const string AzureStorageEmulatorStartupArgs = @"start";
        private const string AzureStorageEmulatorResetArgs = @"clear all";
        private const string AzureStorageEmulatorShutdownArgs = @"stop";

        protected static void InitializeAllTests(TestContext context)
        {
            EnsureAzureStorageEmulatorIsStarted();
        }

        protected static void CleanupAllTests()
        {
            ShutdownAzureStorageEmulator();
        }

        private static void EnsureAzureStorageEmulatorIsStarted()
        {
            ShutdownAzureStorageEmulator();

            ExecuteEmulatorCommand(AzureStorageEmulatorResetArgs);

            var startupArgs = AzureStorageEmulatorStartupArgs;

            ExecuteEmulatorCommand(startupArgs, false);
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        private static void ShutdownAzureStorageEmulator()
        {
            if (IsEmulatorRunning())
            {
                ExecuteEmulatorCommand(AzureStorageEmulatorShutdownArgs);
                if (IsEmulatorRunning())
                {
                    KillEmulatorProcesses();
                }
            }
        }

        private static bool IsEmulatorRunning()
        {
            return Process.GetProcesses()
                .Any(process => process.ProcessName.EqualsIgnoreCase(AzureStorageEmulatorProcessName));
        }

        private static void ExecuteEmulatorCommand(string command, bool waitForCompletion = true)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = command,
                FileName =
                    $"C:\\Program Files (x86)\\Microsoft SDKs\\Azure\\Storage Emulator\\{AzureStorageEmulatorProcessName}.exe",
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
                .Where(process => process.ProcessName.EqualsIgnoreCase(AzureStorageEmulatorProcessName))
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