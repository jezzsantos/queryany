using System;
using System.ServiceProcess;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Storage.IntegrationTests.Redis
{
    public static class RedisInMemStorageBase
    {
        private const string RedisServerServiceName = @"redis";

        public static void InitializeAllTests(TestContext context)
        {
            EnsureRedisServerIsStarted();
        }

        public static void CleanupAllTests()
        {
            ShutdownRedisServer(false);
        }

        private static void EnsureRedisServerIsStarted()
        {
            if (!IsServerRunning())
            {
                StartRedisServer();
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        private static void StartRedisServer()
        {
            using (var controller = new ServiceController(RedisServerServiceName))
            {
                if (controller.Status == ServiceControllerStatus.Stopped)
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
        }

        private static void ShutdownRedisServer(bool forceShutdown)
        {
            using (var controller = new ServiceController(RedisServerServiceName))
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
            using (var controller = new ServiceController(RedisServerServiceName))
            {
                return controller.Status == ServiceControllerStatus.Running;
            }
        }
    }
}