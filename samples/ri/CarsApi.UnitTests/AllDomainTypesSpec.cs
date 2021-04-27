using Api.Common;
using Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CarsApi.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class AllDomainTypesSpec
    {
        private Mock<IDependencyContainer> dependencyContainer;
        private DomainFactory domainFactory;
        private Mock<IRecorder> recorder;

        [TestInitialize]
        public void Initialize()
        {
            this.recorder = new Mock<IRecorder>();
            this.dependencyContainer = new Mock<IDependencyContainer>();
            this.dependencyContainer.Setup(dc => dc.Resolve<IRecorder>()).Returns(this.recorder.Object);
            this.domainFactory = new DomainFactory(this.dependencyContainer.Object);
        }

        [TestMethod]
        public void WhenRegisterAllEntities_ThenAllEntitiesRegistered()
        {
            this.domainFactory.RegisterDomainTypesFromAssemblies(ServiceHost.AssembliesContainingDomainEntities);
        }
    }
}