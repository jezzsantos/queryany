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
using Storage.Interfaces;
using Storage.UnitTests.ReadModels;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GeneralEventStreamStorageSpec
    {
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private EventStreamStateChangedArgs stateChangedEvent;
        private GeneralEventStreamStorage<TestAggregateRoot> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.storage =
                new GeneralEventStreamStorage<TestAggregateRoot>(this.logger.Object, this.domainFactory.Object,
                    this.repository.Object);

            this.stateChangedEvent = null;
            this.storage.OnEventStreamStateChanged += (sender, args) => { this.stateChangedEvent = args; };
        }

        [TestMethod]
        public void WhenLoadAndNoEventsFound_ThenReturnsNewEntity()
        {
            var aggregate = new TestAggregateRoot(null);
            this.repository.Setup(repo =>
                    repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>());
            this.domainFactory.Setup(df => df.RehydrateAggregateRoot(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(aggregate);

            var result = this.storage.Load("anid".ToIdentifier());

            result.Should().Be(aggregate);
            result.LoadedChanges.Should().BeNull();
            this.repository.Setup(repo => repo.Query("acontainername", It.Is<QueryClause<IQueryableEntity>>(q =>
                q.Wheres[0].Condition.FieldName == nameof(EntityEvent.StreamName)
                && q.Wheres[0].Condition.Value.As<string>() == "acontainername_anid"
            ), It.IsAny<RepositoryEntityMetadata>()));
            this.domainFactory.Verify(df => df.RehydrateAggregateRoot(typeof(TestAggregateRoot),
                It.Is<Dictionary<string, object>>(dic =>
                    dic.Count() == 2
                    && dic[nameof(CommandEntity.Id)].As<Identifier>() == "anid"
                    && dic[nameof(CommandEntity.LastPersistedAtUtc)] == null
                )));
        }

        [TestMethod]
        public void WhenLoadAndEventsFound_ThenReturnsNewEntityWithEvents()
        {
            var lastPersisted = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1));
            var aggregate = new TestAggregateRoot(null);
            var events = new List<EntityEvent>
            {
                CreateEventEntity("aneventid1", 1, DateTime.MinValue),
                CreateEventEntity("aneventid2", 2, DateTime.MinValue),
                CreateEventEntity("aneventid3", 3, lastPersisted)
            };
            var queryEntities = events.Select(evt => QueryEntity.FromType(evt)).ToList();
            this.repository.Setup(repo =>
                    repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(queryEntities);
            this.domainFactory.Setup(df => df.RehydrateAggregateRoot(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(aggregate);
            this.domainFactory.Setup(df => df.RehydrateValueObject(typeof(Identifier), It.IsAny<string>()))
                .Returns((Type type, string value) => value.ToIdentifier());
            this.domainFactory.Setup(df => df.RehydrateValueObject(typeof(EventMetadata), It.IsAny<string>()))
                .Returns((Type type, string value) => new EventMetadata(value));

            var result = this.storage.Load("anid".ToIdentifier());

            result.Should().Be(aggregate);
            result.LoadedChanges.Should().BeEquivalentTo(events);
            this.repository.Setup(repo => repo.Query("acontainername", It.Is<QueryClause<IQueryableEntity>>(q =>
                q.Wheres[0].Condition.FieldName == nameof(EntityEvent.StreamName)
                && q.Wheres[0].Condition.Value.As<string>() == "acontainername_anid"
                && q.ResultOptions.OrderBy.By == nameof(IPersistableEntity.LastPersistedAtUtc)
                && q.ResultOptions.OrderBy.Direction == OrderDirection.Ascending
            ), It.IsAny<RepositoryEntityMetadata>()));
            this.domainFactory.Verify(df => df.RehydrateAggregateRoot(typeof(TestAggregateRoot),
                It.Is<Dictionary<string, object>>(dic =>
                    dic.Count() == 2
                    && dic[nameof(CommandEntity.Id)].As<Identifier>() == "anid"
                    && dic[nameof(CommandEntity.LastPersistedAtUtc)].As<DateTime?>() == lastPersisted
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
            this.stateChangedEvent.Should().BeNull();
        }

        [TestMethod]
        public void WhenSaveAndEvents_ThenAddsEventsToRepositoryAndClears()
        {
            var aggregate = new TestAggregateRoot("anid".ToIdentifier())
            {
                Events = new List<EntityEvent>
                {
                    CreateEventEntity("aneventid1", 1, DateTime.UtcNow),
                    CreateEventEntity("aneventid2", 2, DateTime.UtcNow),
                    CreateEventEntity("aneventid3", 3, DateTime.UtcNow)
                }
            };

            this.storage.Save(aggregate);

            this.repository.Verify(
                repo => repo.Add("acontainername_Events", It.IsAny<CommandEntity>()),
                Times.Exactly(3));
            aggregate.ClearedChanges.Should().BeTrue();
            this.stateChangedEvent.Events[0].Id.Should().Be("aneventid1");
            this.stateChangedEvent.Events[1].Id.Should().Be("aneventid2");
            this.stateChangedEvent.Events[2].Id.Should().Be("aneventid3");
        }

        private static EntityEvent CreateEventEntity(string id, long version, DateTime lastPersisted)
        {
            var entity = new EntityEvent();
            entity.SetIdentifier(new FixedIdentifierFactory(id));
            entity.SetEvent("astreamname", "anentitytype", version, new TestEvent());
            entity.LastPersistedAtUtc = lastPersisted;
            return entity;
        }
    }
}