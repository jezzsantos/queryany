using System.Reflection;
using CarsApplication;
using Funq;
using ServiceStack;

namespace CarsApi.IntegrationTests
{
    public class TestServiceHost : AppSelfHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies =
            {typeof(CarsApi.Startup).Assembly};

        public TestServiceHost() : base("CarsApi Testing Service", AssembliesContainingServicesAndDependencies)
        {
        }

        public override void Configure(Container container)
        {
            this.ConfigureServiceHost(true);
            container.AddSingleton<ICarsApplication, CarsApplication.CarsApplication>();
        }
    }
}