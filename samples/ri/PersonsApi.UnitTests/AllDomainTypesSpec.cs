using Api.Common;
using Castle.Core.Logging;
using Domain.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace PersonsApi.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class AllDomainTypesSpec
    {
        private Mock<IDependencyContainer> dependencyContainer;
        private DomainFactory domainFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.dependencyContainer = new Mock<IDependencyContainer>();
            this.dependencyContainer.Setup(dc => dc.Resolve<ILogger>()).Returns(this.logger.Object);
            this.domainFactory = new DomainFactory(this.dependencyContainer.Object);
        }

        [TestMethod]
        public void WhenRegisterAllEntities_ThenAllEntitiesRegistered()
        {
            this.domainFactory.RegisterDomainTypesFromAssemblies(ServiceHost.AssembliesContainingDomainEntities);
        }
    }
}