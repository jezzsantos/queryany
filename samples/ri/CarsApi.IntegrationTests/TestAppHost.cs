using CarsDomain;
using Funq;
using ServiceStack;

namespace CarsApi.IntegrationTests
{
    public class TestAppHost : AppSelfHostBase
    {
        public TestAppHost() : base("CarsApi Testing", typeof(CarsApi.Startup).Assembly) { }

        public override void Configure(Container container)
        {
            container.AddSingleton<Cars>();
        }
    }
}
