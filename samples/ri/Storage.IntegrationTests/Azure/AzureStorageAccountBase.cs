using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Azure
{
    // ReSharper disable once InconsistentNaming
    public static class AzureStorageAccountBase
    {
        private const string EmulatorProcessName = @"AzureStorageEmulator";
        private const string EmulatorStartupArgs = @"start";
        private const string EmulatorResetArgs = @"clear all";
        private const string EmulatorShutdownArgs = @"stop";

        public static void InitializeAllTests(TestContext context)
        {
            EnsureAzureStorageEmulatorIsStarted();
        }

        public static void CleanupAllTests()
        {
            ShutdownAzureStorageEmulator();
        }

        private static void EnsureAzureStorageEmulatorIsStarted()
        {
            ShutdownAzureStorageEmulator();

            ExecuteEmulatorCommand(EmulatorResetArgs);

            var startupArgs = EmulatorStartupArgs;

            ExecuteEmulatorCommand(startupArgs, false);
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        private static void ShutdownAzureStorageEmulator()
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
                FileName =
                    $"C:\\Program Files (x86)\\Microsoft SDKs\\Azure\\Storage Emulator\\{EmulatorProcessName}.exe",
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