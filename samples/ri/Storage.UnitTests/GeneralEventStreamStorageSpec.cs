﻿using System;
using System.Collections.Generic;
using System.Linq;
using Application.Storage.Interfaces;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using QueryAny;
using Storage.Properties;
using Storage.UnitTests.ReadModels;
using UnitTesting.Common;
using Xunit;

namespace Storage.UnitTests
{
    [Trait("Category", "Unit")]
    public class GeneralEventStreamStorageSpec
    {
        private readonly Mock<IDomainFactory> domainFactory;
        private readonly Mock<IRepository> repository;
        private readonly GeneralEventStreamStorage<TestAggregateRoot> storage;
        private EventStreamStateChangedArgs stateChangedEvent;

        public GeneralEventStreamStorageSpec()
        {
            var recorder = new Mock<IRecorder>();
            this.domainFactory = new Mock<IDomainFactory>();
            var migrator = new Mock<IChangeEventMigrator>();
            this.repository = new Mock<IRepository>();
            this.storage =
                new GeneralEventStreamStorage<TestAggregateRoot>(recorder.Object, this.domainFactory.Object,
                    migrator.Object, this.repository.Object);

            this.stateChangedEvent = null;
            this.storage.OnEventStreamStateChanged += (sender, args) => { this.stateChangedEvent = args; };
        }

        [Fact]
        public void WhenLoadAndNoEventsFoundAndWantNull_ThenReturnsNull()
        {
            var aggregate = new TestAggregateRoot(null);
            this.repository.Setup(repo =>
                    repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>());
            this.domainFactory.Setup(df =>
                    df.RehydrateAggregateRoot(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(aggregate);

            var result = this.storage.Load("anid".ToIdentifier(), true);

            result.Should().BeNull();
            this.repository.Setup(repo => repo.Query("acontainername", It.Is<QueryClause<IQueryableEntity>>(q =>
                q.Wheres[0].Condition.FieldName == nameof(EntityEvent.StreamName)
                && q.Wheres[0].Condition.Value.As<string>() == "acontainername_anid"
            ), It.IsAny<RepositoryEntityMetadata>()));
            this.domainFactory.Verify(df => df.RehydrateAggregateRoot(typeof(TestAggregateRoot),
                It.IsAny<Dictionary<string, object>>()), Times.Never);
        }

        [Fact]
        public void WhenLoadAndNoEventsFound_ThenReturnsNewEntity()
        {
            var aggregate = new TestAggregateRoot(null);
            this.repository.Setup(repo =>
                    repo.Query(It.IsAny<string>(), It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>());
            this.domainFactory.Setup(df =>
                    df.RehydrateAggregateRoot(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
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

        [Fact]
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
            this.domainFactory.Setup(df =>
                    df.RehydrateAggregateRoot(It.IsAny<Type>(), It.IsAny<Dictionary<string, object>>()))
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

        [Fact]
        public void WhenSaveAndAggregateHasNoIdentifier_ThenThrowsConflict()
        {
            this.storage
                .Invoking(x => x.Save(new TestAggregateRoot(null)))
                .Should().Throw<ResourceConflictException>();
        }

        [Fact]
        public void WhenSaveAndNoEvents_ThenDoesNothing()
        {
            this.repository.Setup(
                    repo => repo.Query("acontainername_Events", It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity>());

            var aggregate = new TestAggregateRoot("anid".ToIdentifier());
            this.storage.Save(aggregate);

            aggregate.ClearedChanges.Should().BeFalse();
            this.stateChangedEvent.Should().BeNull();
        }

        [Fact]
        public void WhenSaveAndConcurrencyConflict_ThenThrows()
        {
            var @event = CreateEventEntity("aneventid1", 10, DateTime.UtcNow);
            this.repository.Setup(
                    repo => repo.Query("acontainername_Events", It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity> {QueryEntity.FromType(@event)});

            var aggregate = new TestAggregateRoot("anid".ToIdentifier())
            {
                Events = new List<EntityEvent>
                {
                    CreateEventEntity("aneventid1", 1, DateTime.UtcNow),
                    CreateEventEntity("aneventid2", 2, DateTime.UtcNow),
                    CreateEventEntity("aneventid3", 3, DateTime.UtcNow)
                }
            };

            this.storage
                .Invoking(x => x.Save(aggregate))
                .Should().Throw<ResourceConflictException>()
                .WithMessageLike(Resources.GeneralEventStreamStorage_LoadConcurrencyConflictWritingEventStream);
        }

        [Fact]
        public void WhenSaveAndEvents_ThenAddsEventsToRepositoryAndClears()
        {
            var event1 = CreateEventEntity("aneventid1", 1, DateTime.UtcNow);
            this.repository.Setup(
                    repo => repo.Query("acontainername_Events", It.IsAny<QueryClause<EntityEvent>>(),
                        It.IsAny<RepositoryEntityMetadata>()))
                .Returns(new List<QueryEntity> {QueryEntity.FromType(event1)});

            var aggregate = new TestAggregateRoot("anid".ToIdentifier())
            {
                Events = new List<EntityEvent>
                {
                    event1,
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