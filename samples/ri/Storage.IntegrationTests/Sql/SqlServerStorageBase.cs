using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using ServiceStack;

namespace Storage.IntegrationTests.Sql
{
    public static class SqlServerStorageBase
    {
        private const string SqlCommandLineTool = @"SQLCMD";
        private const string CreateDatabaseCommandArgs = "-Q \"CREATE DATABASE {0}\"";
        private const string RegenerateScriptCommandArgs = "-i \"{0}\\Sql\\RegenerateDatabase.sql\"";

        public static void InitializeAllTests(string serviceName, string databaseName)
        {
            EnsureSqlServerServerIsStarted(Environment.CurrentDirectory, serviceName, databaseName);
        }

        public static void CleanupAllTests(string serviceName)
        {
            ShutdownSqlServerServer(serviceName, false);
        }

        private static void EnsureSqlServerServerIsStarted(string deploymentDirectory, string serviceName,
            string databaseName)
        {
            if (!IsServerRunning(serviceName))
            {
                StartSqlServerServer(serviceName);
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

            RegenerateDatabase(databaseName, deploymentDirectory);
        }

        private static void RegenerateDatabase(string databaseName, string scriptPath)
        {
            ExecuteSqlCommand(SqlCommandLineTool, CreateDatabaseCommandArgs.Fmt(databaseName));
            ExecuteSqlCommand(SqlCommandLineTool, RegenerateScriptCommandArgs.Fmt(scriptPath));
        }

        private static void StartSqlServerServer(string serviceName)
        {
            using (var controller = new ServiceController(serviceName))
            {
                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
        }

        private static void ShutdownSqlServerServer(string serviceName, bool forceShutdown)
        {
            using (var controller = new ServiceController(serviceName))
            {
                if (controller.Status == ServiceControllerStatus.Running)
                {
                    if (forceShutdown)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                }
            }
        }

        private static bool IsServerRunning(string serviceName)
        {
            using (var controller = new ServiceController(serviceName))
            {
                return controller.Status == ServiceControllerStatus.Running;
            }
        }

        private static void ExecuteSqlCommand(string command, string arguments, bool waitForCompletion = true)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                Arguments = arguments,
                FileName = command,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardError = true
            });
            if (waitForCompletion)
            {
                process!.WaitForExit();
            }
            if (process!.ExitCode != 0)
            {
                throw new Exception(process.StandardError.ReadToEnd());
            }
        }
    }
}