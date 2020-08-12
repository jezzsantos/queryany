using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class CarEntitySpec
    {
        private CarEntity entity;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.entity = new CarEntity(this.logger.Object, this.identifierFactory.Object);
        }

        [TestMethod]
        public void WhenSetManufacturer_ThenManufactured()
        {
            this.entity.SetManufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]);

            this.entity.Manufacturer.Should()
                .Be(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]));
        }
    }
}