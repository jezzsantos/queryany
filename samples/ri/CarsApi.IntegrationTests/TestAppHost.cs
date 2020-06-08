using System.Reflection;
using CarsDomain;
using Funq;
using ServiceStack;

namespace CarsApi.IntegrationTests
{
    public class TestAppHost : AppSelfHostBase
    {
        private static readonly Assembly[] assembliesContainingServicesAndDependencies = new Assembly[] { typeof(Startup).Assembly };

        public TestAppHost() : base("MyCarsApi Test Service", typeof(CarsApi.Startup).Assembly) { }

        public override void Configure(Container container)
        {
            container.AddSingleton<Cars>();
        }
    }
}
