using Api.Common;
using Common;
using Moq;
using Xunit;

namespace PersonsApi.UnitTests
{
    [Trait("Category", "Unit")]
    public class AllDomainTypesSpec
    {
        private readonly DomainFactory domainFactory;

        public AllDomainTypesSpec()
        {
            var recorder = new Mock<IRecorder>();
            var dependencyContainer = new Mock<IDependencyContainer>();
            dependencyContainer.Setup(dc => dc.Resolve<IRecorder>()).Returns(recorder.Object);
            this.domainFactory = new DomainFactory(dependencyContainer.Object);
        }

        [Fact]
        public void WhenRegisterAllEntities_ThenAllEntitiesRegistered()
        {
            this.domainFactory.RegisterDomainTypesFromAssemblies(ServiceHost.AssembliesContainingDomainEntities);
        }
    }
}