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

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class GeneralEventingStorageSpec
    {
        private GeneralEventingStorage<TestAggregateRoot> commandStorage;
        private Mock<IDomainFactory> domainFactory;
        private Mock<ILogger> logger;
        private Mock<IRepository> repository;
        private EventStreamStateChangedArgs stateChangedEvent;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.domainFactory = new Mock<IDomainFactory>();
            this.repository = new Mock<IRepository>();
            this.commandStorage =
                new GeneralEventingStorage<TestAggregateRoot>(this.logger.Object, this.domainFactory.Object,
                    this.repository.Object);

            this.stateChangedEvent = null;
            this.commandStorage.OnEventStreamStateChanged += (sender, args) => { this.stateChangedEvent = args; };
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

            var result = this.commandStorage.Load("anid".ToIdentifier());

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
                CreateEventEntity("aneventid1", 1, DateTime.MinValue),
                CreateEventEntity("aneventid2", 2, DateTime.MinValue),
                CreateEventEntity("aneventid3", 3, lastPersisted)
            };
            this.repository.Setup(repo => repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EventEntity>>(),
                    this.domainFactory.Object))
                .Returns(events);
            this.domainFactory.Setup(df => df.RehydrateEntity(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(aggregate);

            var result = this.commandStorage.Load("anid".ToIdentifier());

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
            this.stateChangedEvent.Should().BeNull();
        }

        [TestMethod]
        public void WhenSaveAndEvents_ThenAddsEventsToRepositoryAndClears()
        {
            var aggregate = new TestAggregateRoot("anid".ToIdentifier())
            {
                Events = new List<EventEntity>
                {
                    CreateEventEntity("aneventid1", 1, DateTime.UtcNow),
                    CreateEventEntity("aneventid2", 2, DateTime.UtcNow),
                    CreateEventEntity("aneventid3", 3, DateTime.UtcNow)
                }
            };

            this.commandStorage.Save(aggregate);

            this.repository.Verify(repo => repo.Add("acontainername_Events", It.IsAny<EventEntity>()),
                Times.Exactly(3));
            aggregate.ClearedChanges.Should().BeTrue();
            this.stateChangedEvent.Should().BeEquivalentTo(new EventStreamStateChangedArgs("astreamname",
                new List<EventStreamStateChangeEvent>
                {
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid1",
                        Type = "atypename",
                        Data = "somejson",
                        Version = 1
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid2",
                        Type = "atypename",
                        Data = "somejson",
                        Version = 2
                    },
                    new EventStreamStateChangeEvent
                    {
                        Id = "aneventid3",
                        Type = "atypename",
                        Data = "somejson",
                        Version = 3
                    }
                }));
        }

        private static EventEntity CreateEventEntity(string id, long version, DateTime lastPersisted)
        {
            var entity = new EventEntity(new FixedIdentifierFactory(id));
            entity.Rehydrate(new Dictionary<string, object>
            {
                {nameof(IIdentifiableEntity.Id), id.ToIdentifier()},
                {nameof(IPersistableEntity.LastPersistedAtUtc), lastPersisted},
                {nameof(EventEntity.StreamName), "astreamname"},
                {nameof(EventEntity.Version), version},
                {nameof(EventEntity.TypeName), "atypename"},
                {nameof(EventEntity.Data), "somejson"}
            });

            return entity;
        }
    }
}