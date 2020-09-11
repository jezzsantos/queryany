using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;
using Storage.Interfaces;
using Storage.ReadModels;

namespace Storage.UnitTests.ReadModels
{
    [TestClass, TestCategory("Unit")]
    public class ReadModelCheckpointStoreSpec
    {
        private Mock<ICommandStorage<Checkpoint>> commandStorage;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;
        private Mock<IQueryStorage<Checkpoint>> queryStorage;
        private ReadModelCheckpointStore store;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier);
            this.commandStorage = new Mock<ICommandStorage<Checkpoint>>();
            this.queryStorage = new Mock<IQueryStorage<Checkpoint>>();
            this.queryStorage.Setup(qs => qs.Query(It.IsAny<QueryClause<Checkpoint>>()))
                .Returns(new QueryResults<Checkpoint>(new List<Checkpoint>()));
            this.store = new ReadModelCheckpointStore(this.logger.Object, this.idFactory.Object,
                this.commandStorage.Object, this.queryStorage.Object);
        }

        [TestMethod]
        public void WhenLoadCheckpointAndNotExists_ThenReturns1()
        {
            var result = this.store.LoadCheckpoint("astreamname");

            result.Should().Be(1);
        }

        [TestMethod]
        public void WhenLoadCheckpointAndExists_ThenReturnsPosition()
        {
            this.queryStorage.Setup(qs => qs.Query(It.IsAny<QueryClause<Checkpoint>>()))
                .Returns(new QueryResults<Checkpoint>(new List<Checkpoint>
                {
                    new Checkpoint {Position = 10}
                }));
            var result = this.store.LoadCheckpoint("astreamname");

            result.Should().Be(10);
        }

        [TestMethod]
        public void WhenSaveCheckpointAndNotExists_ThenSavesPosition()
        {
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anewid".ToIdentifier);
            this.store.SaveCheckpoint("astreamname", 10);

            this.commandStorage.Verify(cs => cs.Upsert(It.Is<Checkpoint>(cp =>
                cp.Id == "anewid"
                && cp.Position == 10
                && cp.StreamName == "astreamname"
            )));
        }

        [TestMethod]
        public void WhenSaveCheckpointAndExists_ThenSavesPosition()
        {
            var existing = new Checkpoint {Position = 1};
            existing.SetIdentifier(this.idFactory.Object);
            this.queryStorage.Setup(qs => qs.Query(It.IsAny<QueryClause<Checkpoint>>()))
                .Returns(new QueryResults<Checkpoint>(new List<Checkpoint>
                {
                    existing
                }));
            this.store.SaveCheckpoint("astreamname", 10);

            this.commandStorage.Verify(cs => cs.Upsert(It.Is<Checkpoint>(cp =>
                cp.Id == "anid"
                && cp.Position == 10
            )));
        }
    }
}