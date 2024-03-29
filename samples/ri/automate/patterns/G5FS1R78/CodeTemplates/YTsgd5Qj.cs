using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack;

namespace {{DomainName | string.pascalplural}}ApiHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseModularStartup<Startup>()
                .ConfigureLogging((context, builder) => builder.AddConsole())
                .Build();
        }
    }
}