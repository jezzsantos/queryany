using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueryAny;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GenericCommandStoragePolicySpec
    {
        private GenericCommandStorage<TestEntity> commandStorage;
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.commandStorage =
                new TestCommandStorage(this.logger.Object, this.domainFactory.Object, this.repository.Object);
        }

        [TestMethod]
        public void WhenDelete_ThenRemovesFromRepository()
        {
            this.commandStorage.Delete("anid".ToIdentifier());

            this.repository.Verify(repo => repo.Remove<TestEntity>("acontainername", "anid".ToIdentifier()));
        }

        [TestMethod]
        public void WhenGet_ThenRetrievesFromRepository()
        {
            this.commandStorage.Get("anid".ToIdentifier());

            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdNotExists_ThenThrowsNotFound()
        {
            this.commandStorage
                .Invoking(x => x.Upsert(new TestEntity(null)))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdIsEmpty_ThenThrowsNotFound()
        {
            this.commandStorage
                .Invoking(x => x.Upsert(new TestEntity(Identifier.Empty())))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityNotExists_ThenAddsToRepository()
        {
            var entity = new TestEntity("anid".ToIdentifier());

            this.commandStorage.Upsert(entity);

            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
            this.repository.Verify(repo => repo.Add("acontainername", entity));
        }

        [TestMethod]
        public void WhenUpsertAndEntityExists_ThenReplacesInRepository()
        {
            var upsertedEntity = new TestEntity("anid".ToIdentifier()) {AStringValue = "anewvalue"};
            var fetchedEntity = new TestEntity("anid".ToIdentifier());
            var updatedEntity = new TestEntity("anid".ToIdentifier());
            this.repository.Setup(repo =>
                    repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object))
                .Returns(fetchedEntity);
            this.repository.Setup(repo =>
                    repo.Replace("acontainername", "anid".ToIdentifier(), fetchedEntity, this.domainFactory.Object))
                .Returns(updatedEntity);

            var result = this.commandStorage.Upsert(upsertedEntity);

            result.Should().Be(updatedEntity);
            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
            this.repository.Verify(repo =>
                repo.Replace("acontainername", "anid".ToIdentifier(), It.Is<TestEntity>(entity =>
                    entity.AStringValue == "anewvalue"
                ), this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenCount_ThenGetsCountFromRepo()
        {
            this.commandStorage.Count();

            this.repository.Verify(repo => repo.Count("acontainername"));
        }

        [TestMethod]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.commandStorage.DestroyAll();

            this.repository.Verify(repo => repo.DestroyAll("acontainername"));
        }

        [TestMethod]
        public void WhenLoadAndNoEventsFound_ThenReturnsNewEntity()
        {
            var aggregate = new TestAggregateRoot(null);
            this.repository.Setup(repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EventEntity>>(),
                    this.domainFactory.Object))
                .Returns(new List<EventEntity>());
            this.domainFactory.Setup(df => df.RehydrateEntity(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(aggregate);

            var result = this.commandStorage.Load<TestAggregateRoot>("anid".ToIdentifier());

            result.Should().Be(aggregate);
            result.LoadedChanges.Should().BeNull();
            this.repository.Setup(repo => repo.Query("acontainername", It.Is<QueryClause<EventEntity>>(q =>
                    q.Wheres[0].Condition.FieldName == nameof(EventEntity.StreamName)
                    && q.Wheres[0].Condition.Value.As<string>() == "acontainername_anid"
                ),
                this.domainFactory.Object));
            this.domainFactory.Verify(df => df.RehydrateEntity(typeof(TestAggregateRoot),
                It.Is<Dictionary<string, object>>(dic =>
                    dic.Count() == 2
                    && dic[nameof(IIdentifiableEntity.Id)].As<Identifier>() == "anid"
                    && dic[nameof(IPersistableEntity.LastPersistedAtUtc)] == null
                )));
        }

        [TestMethod]
        public void WhenLoadAndEventsFound_ThenReturnsNewEntityWithEvents()
        {
            var lastPersisted = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            var aggregate = new TestAggregateRoot(null);
            var events = new List<EventEntity>
            {
                CreateEventEntity("aneventid1", DateTime.MinValue),
                CreateEventEntity("aneventid2", DateTime.MinValue),
                CreateEventEntity("aneventid3", lastPersisted)
            };
            this.repository.Setup(repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EventEntity>>(),
                    this.domainFactory.Object))
                .Returns(events);
            this.domainFactory.Setup(df => df.RehydrateEntity(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(aggregate);

            var result = this.commandStorage.Load<TestAggregateRoot>("anid".ToIdentifier());

            result.Should().Be(aggregate);
            result.LoadedChanges.Should().BeEquivalentTo(events);
            this.repository.Setup(repo => repo.Query("acontainername", It.Is<QueryClause<EventEntity>>(q =>
                    q.Wheres[0].Condition.FieldName == nameof(EventEntity.StreamName)
                    && q.Wheres[0].Condition.Value.As<string>() == "acontainername_anid"
                    && q.ResultOptions.OrderBy.By == nameof(IPersistableEntity.LastPersistedAtUtc)
                    && q.ResultOptions.OrderBy.Direction == OrderDirection.Ascending
                ),
                this.domainFactory.Object));
            this.domainFactory.Verify(df => df.RehydrateEntity(typeof(TestAggregateRoot),
                It.Is<Dictionary<string, object>>(dic =>
                    dic.Count() == 2
                    && dic[nameof(IIdentifiableEntity.Id)].As<Identifier>() == "anid"
                    && dic[nameof(IPersistableEntity.LastPersistedAtUtc)].As<DateTime?>() == lastPersisted
                )));
        }

        [TestMethod]
        public void WhenSaveAndAggregateHasNoIdentifier_ThenThrowsConflict()
        {
            this.commandStorage
                .Invoking(x => x.Save(new TestAggregateRoot(null)))
                .Should().Throw<ResourceConflictException>();
        }

        [TestMethod]
        public void WhenSaveAndNoEvents_ThenDoesNothing()
        {
            var aggregate = new TestAggregateRoot("anid".ToIdentifier());
            this.commandStorage.Save(aggregate);

            aggregate.ClearedChanges.Should().BeFalse();
        }

        [TestMethod]
        public void WhenSaveAndEvents_ThenAddsEventsToRepositoryAndClears()
        {
            var aggregate = new TestAggregateRoot("anid".ToIdentifier())
            {
                Events = new List<EventEntity>
                {
                    CreateEventEntity("aneventid1", DateTime.UtcNow),
                    CreateEventEntity("aneventid2", DateTime.UtcNow),
                    CreateEventEntity("aneventid3", DateTime.UtcNow)
                }
            };

            this.commandStorage.Save(aggregate);

            this.repository.Verify(repo => repo.Add("acontainername_Events", It.IsAny<EventEntity>()),
                Times.Exactly(3));
            aggregate.ClearedChanges.Should().BeTrue();
        }

        private static EventEntity CreateEventEntity(string id, DateTime lastPersisted)
        {
            var entity = new EventEntity(new FixedIdentifierFactory(id));

            entity.Rehydrate(new Dictionary<string, object>
            {
                {nameof(IIdentifiableEntity.Id), id.ToIdentifier()},
                {nameof(IPersistableEntity.LastPersistedAtUtc), lastPersisted}
            });

            return entity;
        }
    }
}