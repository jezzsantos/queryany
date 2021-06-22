using System;
using System.Net;
using System.Net.Sockets;
using Funq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack;
using Xunit;

namespace IntegrationTesting.Common
{
    [CollectionDefinition("ThisAssembly")]
    public class MyCollectionDefinition<TStartup> : ICollectionFixture<ApiSpecSetup<TStartup>> where TStartup : class
    {
    }

    public class ApiSpecSetup<TStartup> : IDisposable where TStartup : class
    {
        private readonly Container container;
        private readonly IWebHost webHost;

        public ApiSpecSetup()
        {
            ServiceStackHost.Instance?.Dispose();

            ServiceUrl = $"https://localhost:{GetNextAvailablePort()}/";
            this.webHost = WebHost.CreateDefaultBuilder(null)
                .UseModularStartup<TStartup>()
                .UseUrls(ServiceUrl)
                .UseKestrel()
                .ConfigureLogging((ctx, builder) => builder.AddConsole())
                .Build();
            this.webHost.Start();

            this.container = HostContext.Container;
            Api = ApiClient.Create(ServiceUrl);
        }

        public JsonServiceClient Api { get; }

        private string ServiceUrl { get; }

        public void Dispose()
        {
            this.webHost?.StopAsync().GetAwaiter().GetResult();
            this.webHost?.Dispose();
        }

        public TService Resolve<TService>()
        {
            return this.container.Resolve<TService>();
        }

        private static int GetNextAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}