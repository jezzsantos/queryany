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
    public class GenericStoragePolicySpec
    {
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private GenericStorage<TestEntity> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.storage = new TestStorage(this.logger.Object, this.domainFactory.Object, this.repository.Object);
        }

        [TestMethod]
        public void WhenAddAndEntityIdNotExists_ThenThrowsConflict()
        {
            this.storage
                .Invoking(x => x.Add(new TestEntity(null)))
                .Should().Throw<ResourceConflictException>();
        }

        [TestMethod]
        public void WhenAddAndEntityExists_ThenAddsAndRetrievesFromRepository()
        {
            var entity = new TestEntity("anid".ToIdentifier());

            this.storage.Add(entity);

            this.repository.Verify(repo => repo.Add("acontainername", entity));
            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenDelete_ThenRemovesFromRepository()
        {
            this.storage.Delete("anid".ToIdentifier());

            this.repository.Verify(repo => repo.Remove<TestEntity>("acontainername", "anid".ToIdentifier()));
        }

        [TestMethod]
        public void WhenGet_ThenRetrievesFromRepository()
        {
            this.storage.Get("anid".ToIdentifier());

            this.repository.Verify(repo =>
                repo.Retrieve<TestEntity>("acontainername", "anid".ToIdentifier(), this.domainFactory.Object));
        }

        [TestMethod]
        public void WhenUpsertAndEntityIdNotExists_ThenThrowsNotFound()
        {
            this.storage
                .Invoking(x => x.Upsert(new TestEntity(null)))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpsertAndEntityNotExists_ThenAddsToRepository()
        {
            var entity = new TestEntity("anid".ToIdentifier());

            this.storage.Upsert(entity);

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

            var result = this.storage.Upsert(upsertedEntity);

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
            this.storage.Count();

            this.repository.Verify(repo => repo.Count("acontainername"));
        }

        [TestMethod]
        public void WhenDestroyAll_ThenGetsCountFromRepo()
        {
            this.storage.DestroyAll();

            this.repository.Verify(repo => repo.DestroyAll("acontainername"));
        }

        [TestMethod]
        public void WhenQueryWithNullQuery_ThenReturnsEmptyResults()
        {
            var result = this.storage.Query(null);

            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
            this.repository.Verify(
                repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<TestEntity>>(), It.IsAny<IDomainFactory>()),
                Times.Never);
        }

        [TestMethod]
        public void WhenQueryWithEmptyQuery_ThenReturnsEmptyResults()
        {
            var query = Query.Empty<TestEntity>();
            var result = this.storage.Query(query);

            result.Should().NotBeNull();
            result.Results.Should().BeEmpty();
            this.repository.Verify(
                repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<TestEntity>>(), It.IsAny<IDomainFactory>()),
                Times.Never);
        }

        [TestMethod]
        public void WhenQuery_ThenQueriesRepo()
        {
            var query = Query.From<TestEntity>().WhereAll();
            var results = new List<TestEntity>();
            this.repository.Setup(repo => repo.Query("acontainername", query, this.domainFactory.Object))
                .Returns(results);

            var result = this.storage.Query(query);

            result.Results.Should().BeEquivalentTo(results);
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

            var result = this.storage.Load<TestAggregateRoot>("anid".ToIdentifier());

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

            var result = this.storage.Load<TestAggregateRoot>("anid".ToIdentifier());

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
            this.storage
                .Invoking(x => x.Save(new TestAggregateRoot(null)))
                .Should().Throw<ResourceConflictException>();
        }

        [TestMethod]
        public void WhenSaveAndNoEvents_ThenDoesNothing()
        {
            var aggregate = new TestAggregateRoot("anid".ToIdentifier());
            this.storage.Save(aggregate);

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

            this.storage.Save(aggregate);

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