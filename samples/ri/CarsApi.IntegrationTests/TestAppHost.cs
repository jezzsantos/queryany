using System.Reflection;
using CarsDomain;
using Funq;
using ServiceStack;

namespace CarsApi.IntegrationTests
{
    public class TestAppHost : AppSelfHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies =
            {typeof(CarsApi.Startup).Assembly};

        public TestAppHost() : base("MyCarsApi Test Service", AssembliesContainingServicesAndDependencies)
        {
        }

        public override void Configure(Container container)
        {
            container.AddSingleton<Cars>();
        }
    }
}