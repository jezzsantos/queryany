using System.Reflection;
using Api.Common;
using Funq;
using PersonsApplication;
using ServiceStack;

namespace PersonsApi.IntegrationTests
{
    public class TestServiceHost : AppSelfHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies =
            {typeof(PersonsApi.Startup).Assembly};

        public TestServiceHost() : base("PersonsApi Testing Service", AssembliesContainingServicesAndDependencies)
        {
        }

        public override void Configure(Container container)
        {
            this.ConfigureServiceHost(true);
            container.AddSingleton<IPersonsApplication, PersonsApplication.PersonsApplication>();
        }
    }
}