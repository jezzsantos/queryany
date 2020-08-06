using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Sql
{
    public abstract class SqlServerStorageBaseSpec : AnyStorageBaseSpec
    {
        private const string SqlServerServerServiceName = @"MSSQLSERVER";
        private const string SqlCommandLineTool = @"SQLCMD";
        private const string CreateDatabaseCommandArgs = "-Q \"CREATE DATABASE {0}\"";
        private const string RegenerateScriptCommandArgs = "-i \"{0}\\Sql\\RegenerateDatabase.sql\"";

        protected static void InitializeAllTests(TestContext context, string databaseName)
        {
            EnsureSqlServerServerIsStarted(context.DeploymentDirectory, databaseName);
        }

        protected static void CleanupAllTests()
        {
            ShutdownSqlServerServer(false);
        }

        private static void EnsureSqlServerServerIsStarted(string deploymentDirectory, string databaseName)
        {
            if (!IsServerRunning())
            {
                StartSqlServerServer();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

            RegenerateDatabase(databaseName, deploymentDirectory);
        }

        private static void RegenerateDatabase(string databaseName, string scriptPath)
        {
            ExecuteSqlCommand(SqlCommandLineTool, CreateDatabaseCommandArgs.Format(databaseName));
            ExecuteSqlCommand(SqlCommandLineTool, RegenerateScriptCommandArgs.Format(scriptPath));
        }

        private static void StartSqlServerServer()
        {
            using (var controller = new ServiceController(SqlServerServerServiceName))
            {
                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
        }

        private static void ShutdownSqlServerServer(bool forceShutdown)
        {
            using (var controller = new ServiceController(SqlServerServerServiceName))
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

        private static bool IsServerRunning()
        {
            using (var controller = new ServiceController(SqlServerServerServiceName))
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