using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;
using Storage.Interfaces.ReadModels;
using Storage.ReadModels;

namespace Storage.UnitTests.ReadModels
{
    [TestClass, TestCategory("Unit")]
    public class ReadModelCheckpointStoreSpec
    {
        private Mock<IDomainFactory> domainFactory;
        private Mock<IIdentifierFactory> idFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private ReadModelCheckpointStore store;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier);
            this.domainFactory = new Mock<IDomainFactory>();
            this.domainFactory.Setup(df => df.RehydrateValueObject(typeof(Identifier), It.IsAny<string>()))
                .Returns((Type type, string value) => Identifier.Create(value));
            this.repository = new Mock<IRepository>();
            this.repository.Setup(repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<Checkpoint>>(),
                    It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>());
            this.store = new ReadModelCheckpointStore(this.logger.Object, this.idFactory.Object,
                this.domainFactory.Object, this.repository.Object);
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
            this.repository.Setup(repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<Checkpoint>>(),
                    It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>
                {
                    QueryEntity.FromType(new Checkpoint {Position = 10})
                });

            var result = this.store.LoadCheckpoint("astreamname");

            result.Should().Be(10);
        }

        [TestMethod]
        public void WhenSaveCheckpointAndNotExists_ThenSavesPosition()
        {
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anewid".ToIdentifier);

            this.store.SaveCheckpoint("astreamname", 10);

            this.repository.Verify(cs => cs.Add("checkpoints", It.Is<CommandEntity>(
                entity =>
                    entity.Id == "anewid"
                    && (long) entity.Properties[nameof(Checkpoint.Position)] == 10
                    && entity.Properties[nameof(Checkpoint.StreamName)].ToString() == "astreamname"
            )));
        }

        [TestMethod]
        public void WhenSaveCheckpointAndExists_ThenSavesPosition()
        {
            var existing = new Checkpoint {Id = "anid".ToIdentifier(), Position = 1};
            this.repository.Setup(repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<Checkpoint>>(),
                    It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>
                {
                    QueryEntity.FromType(existing)
                });

            this.store.SaveCheckpoint("astreamname", 10);

            this.repository.Verify(cs => cs.Replace("checkpoints", "anid".ToIdentifier(), It.Is<CommandEntity>(
                entity =>
                    entity.Id == "anid"
                    && (long) entity.Properties[nameof(Checkpoint.Position)] == 10
            )));
        }
    }
}