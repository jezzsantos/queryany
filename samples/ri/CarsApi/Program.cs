using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using ServiceStack;

namespace CarsApi
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
                .Build();
        }
    }
}