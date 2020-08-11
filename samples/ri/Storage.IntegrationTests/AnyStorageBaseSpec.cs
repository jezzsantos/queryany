using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    public abstract class AnyStorageBaseSpec

    {
        protected static readonly ILogger Logger = new Logger<AnyStorageBaseSpec>(new NullLoggerFactory());
        private IStorage<FirstJoiningTestEntity> firstJoiningStorage;
        private IStorage<SecondJoiningTestEntity> secondJoiningStorage;
        private IStorage<TestEntity> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.storage = GetStore(typeof(TestEntity).GetEntityNameSafe(), TestEntity.GetFactory());
            this.storage.DestroyAll();
            this.firstJoiningStorage =
                GetStore(typeof(FirstJoiningTestEntity).GetEntityNameSafe(), FirstJoiningTestEntity.GetFactory());
            this.firstJoiningStorage.DestroyAll();
            this.secondJoiningStorage = GetStore(typeof(SecondJoiningTestEntity).GetEntityNameSafe(),
                SecondJoiningTestEntity.GetFactory());
            this.secondJoiningStorage.DestroyAll();
        }

        protected abstract IStorage<TEntity> GetStore<TEntity>(string containerName,
            EntityFactory<TEntity> entityFactory)
            where TEntity : IPersistableEntity;

        [TestMethod]
        public void WhenAddAndEntityNotExists_ThenAddsNew()
        {
            this.storage.Count().Should().Be(0);
            var entity = new TestEntity();

            this.storage.Add(entity);

            this.storage.Count().Should().Be(1);

            var added = this.storage.Get(entity.Id);
            added.Id.Should().Be(entity.Id);
            added.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            added.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            added.LastModifiedAtUtc.Should().BeAfter(added.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            var entity = new TestEntity();
            this.storage.Add(entity);

            this.storage.Delete(entity.Id);

            this.storage.Count().Should().Be(0);
        }

        [TestMethod]
        public void WhenDeleteAndEntityNotExists_ThenReturns()
        {
            this.storage.Delete("anid".ToIdentifier());

            this.storage.Count().Should().Be(0);
        }

        [TestMethod]
        public void WhenDeleteAndIdIsEmpty_ThenThrows()
        {
            this.storage.Invoking(x => x.Delete(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var entity = this.storage.Get("anid".ToIdentifier());

            entity.Should().BeNull();
        }

        [TestMethod]
        public void WhenGetAndExists_ThenReturnsEntity()
        {
            var entity = new TestEntity
            {
                ABinaryValue = new byte[] {0x01},
                ABooleanValue = true,
                ADoubleValue = 0.1,
                AGuidValue = Guid.Empty,
                AIntValue = 1,
                ALongValue = 2,
                AStringValue = "astringvalue",
                ADateTimeUtcValue = DateTime.Today.ToUniversalTime(),
                ADateTimeOffsetUtcValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                AComplexNonValueObjectValue = new ComplexNonValueObject
                {
                    APropertyValue = "avalue"
                },
                AComplexValueObjectValue = ComplexValueObject.Create("avalue", 25, true)
            };

            this.storage.Add(entity);

            var result = this.storage.Get(entity.Id);

            result.Id.Should().Be(entity.Id);
            result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
            result.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastModifiedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(true);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(1);
            result.ALongValue.Should().Be(2);
            result.ADoubleValue.Should().Be(0.1);
            result.AStringValue.Should().Be("astringvalue");
            result.ADateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ADateTimeUtcValue.Kind.Should().Be(DateTimeKind.Utc);
            result.ADateTimeOffsetUtcValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.AComplexNonValueObjectValue.ToString().Should()
                .Be(new ComplexNonValueObject {APropertyValue = "avalue"}.ToString());
            result.AComplexValueObjectValue.Should().Be(ComplexValueObject.Create("avalue", 25, true));
        }

        [TestMethod]
        public void WhenGetAndExistsWithDefaultValues_ThenReturnsEntity()
        {
            var entity = new TestEntity
            {
                ABinaryValue = default,
                ABooleanValue = default,
                ADoubleValue = default,
                AGuidValue = default,
                AIntValue = default,
                ALongValue = default,
                AStringValue = default,
                ADateTimeUtcValue = default,
                ADateTimeOffsetUtcValue = default,
                AComplexNonValueObjectValue = default,
                AComplexValueObjectValue = default
            };

            this.storage.Add(entity);

            var result = this.storage.Get(entity.Id);

            result.Id.Should().Be(entity.Id);
            result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.CreatedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
            result.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastModifiedAtUtc.Kind.Should().Be(DateTimeKind.Utc);
            result.ABinaryValue.Should().BeNull();
            result.ABooleanValue.Should().Be(default);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(default);
            result.ALongValue.Should().Be(default);
            result.ADoubleValue.Should().Be(default);
            result.AStringValue.Should().Be(default);
            result.ADateTimeUtcValue.Should().Be(DateTime.MinValue);
            result.ADateTimeUtcValue.Kind.Should().Be(DateTimeKind.Unspecified);
            result.ADateTimeOffsetUtcValue.Should().Be(default);
            result.AComplexNonValueObjectValue.Should().Be(default);
            result.AComplexValueObjectValue.Should().Be(default);
        }

        [TestMethod]
        public void WhenGetAndIdIsEmpty_ThenThrows()
        {
            this.storage.Invoking(x => x.Get(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUpdateAndExists_ThenReturnsUpdated()
        {
            var entity = new TestEntity();
            this.storage.Add(entity);

            entity.AStringValue = "updated";
            var updated = this.storage.Update(entity);

            updated.Id.Should().Be(entity.Id);
            updated.AStringValue.Should().Be("updated");
            updated.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            updated.LastModifiedAtUtc.Should().BeAfter(updated.CreatedAtUtc,
                $"{nameof(IModifiableEntity.LastModifiedAtUtc)} ({updated.LastModifiedAtUtc:O}) is not after {nameof(IModifiableEntity.CreatedAtUtc)} ({updated.CreatedAtUtc:O})");
        }

        [TestMethod]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            var entity = new TestEntity
            {
                AStringValue = "updated"
            };

            this.storage.Invoking(x => x.Update(entity))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenUpdateAndEmptyId_ThenThrows()
        {
            var entity = new TestEntity();

            this.storage.Invoking(x => x.Update(entity))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenCountAndEmpty_ThenReturnsZero()
        {
            var count = this.storage.Count();

            count.Should().Be(0);
        }

        [TestMethod]
        public void WhenCountAndNotEmpty_ThenReturnsCount()
        {
            this.storage.Add(new TestEntity());
            this.storage.Add(new TestEntity());

            var count = this.storage.Count();

            count.Should().Be(2);
        }

        [TestMethod]
        public void WhenQueryAndQueryIsNull_ThenThrows()
        {
            this.storage.Invoking(x => x.Query(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenQueryAndEmpty_ThenReturnsEmptyResults()
        {
            var query = Query.Empty<TestEntity>();
            this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndWhereAll_ThenReturnsAllResults()
        {
            var query = Query.From<TestEntity>()
                .WhereAll();
            var entity = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity.Id);
        }

        [TestMethod]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndNoMatch_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "anothervalue");
            this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndMatchOne_ThenReturnsResult()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var entity = new TestEntity
            {
                AStringValue = "avalue"
            };
            this.storage.Add(entity);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity.Id);
        }

        [TestMethod]
        public void WhenQueryAndMatchMany_ThenReturnsResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var entity1 = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });
            var entity2 = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var entity2 = new TestEntity {AStringValue = "avalue2"};
            this.storage.Add(entity2);
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, entity2.Id);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var entity2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var entity2 = this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNotNullStringValue_ThenReturnsResult()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var entity2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTime1 = DateTimeOffset.UtcNow;
            var dateTime2 = DateTimeOffset.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeOffsetUtcValue = dateTime1});
            var entity2 = this.storage.Add(new TestEntity {ADateTimeOffsetUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var entity2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueGreaterThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var entity2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThan, dateTime1);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var entity2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThanEqualTo, dateTime1);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueLessThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThan, dateTime2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var entity2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThanEqualTo, dateTime2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueNotEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.NotEqualTo, dateTime2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ABooleanValue = false});
            var entity2 = this.storage.Add(new TestEntity {ABooleanValue = true});
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var entity2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var entity2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var entity1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var entity2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var entity1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var entity1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var entity2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var entity1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ALongValue = 1});
            var entity2 = this.storage.Add(new TestEntity {ALongValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ADoubleValue = 1.0});
            var entity2 = this.storage.Add(new TestEntity {ADoubleValue = 2.0});
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForGuidValue_ThenReturnsResult()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.storage.Add(new TestEntity {AGuidValue = guid1});
            var entity2 = this.storage.Add(new TestEntity {AGuidValue = guid2});
            var query = Query.From<TestEntity>().Where(e => e.AGuidValue, ConditionOperator.EqualTo, guid2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForBinaryValue_ThenReturnsResult()
        {
            var binary1 = new byte[] {0x01};
            var binary2 = new byte[] {0x01, 0x02};
            this.storage.Add(new TestEntity {ABinaryValue = binary1});
            var entity2 = this.storage.Add(new TestEntity {ABinaryValue = binary2});
            var query = Query.From<TestEntity>().Where(e => e.ABinaryValue, ConditionOperator.EqualTo, binary2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            var complex2 = new ComplexNonValueObject {APropertyValue = "avalue2"};
            this.storage.Add(new TestEntity {AComplexNonValueObjectValue = complex1});
            var entity2 = this.storage.Add(new TestEntity {AComplexNonValueObjectValue = complex2});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.EqualTo, complex2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
            results.Results[0].AComplexNonValueObjectValue.ToString().Should().Be(complex2.ToString());
        }

        [TestMethod]
        public void WhenQueryForNullComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            this.storage.Add(new TestEntity {AComplexNonValueObjectValue = complex1});
            var entity2 = this.storage.Add(new TestEntity {AComplexNonValueObjectValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
            results.Results[0].AComplexNonValueObjectValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            var entity1 = this.storage.Add(new TestEntity {AComplexNonValueObjectValue = complex1});
            this.storage.Add(new TestEntity {AComplexNonValueObjectValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[0].AComplexNonValueObjectValue.ToString().Should().Be(complex1.ToString());
        }

        [TestMethod]
        public void WhenQueryForComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            var complex2 = ComplexValueObject.Create("avalue2", 50, false);
            this.storage.Add(new TestEntity {AComplexValueObjectValue = complex1});
            var entity2 = this.storage.Add(new TestEntity {AComplexValueObjectValue = complex2});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.EqualTo, complex2);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
            results.Results[0].AComplexValueObjectValue.Should().Be(complex2);
        }

        [TestMethod]
        public void WhenQueryForNullComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            this.storage.Add(new TestEntity {AComplexValueObjectValue = complex1});
            var entity2 = this.storage.Add(new TestEntity {AComplexValueObjectValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity2.Id);
            results.Results[0].AComplexValueObjectValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            var entity1 = this.storage.Add(new TestEntity {AComplexValueObjectValue = complex1});
            this.storage.Add(new TestEntity {AComplexValueObjectValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[0].AComplexValueObjectValue.Should().Be(complex1);
        }

        [TestMethod]
        public void WhenQueryAndNoSelects_ThenReturnsResultWithAllPropertiesPopulated()
        {
            var entity = new TestEntity
            {
                ABinaryValue = new byte[] {0x01},
                ABooleanValue = true,
                ADoubleValue = 0.1,
                AGuidValue = Guid.Empty,
                AIntValue = 1,
                ALongValue = 2,
                AStringValue = "astringvalue",
                ADateTimeUtcValue = DateTime.Today.ToUniversalTime(),
                ADateTimeOffsetUtcValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                AComplexNonValueObjectValue = new ComplexNonValueObject
                {
                    APropertyValue = "avalue"
                },
                AComplexValueObjectValue = ComplexValueObject.Create("avalue", 25, true)
            };

            this.storage.Add(entity);
            var query = Query.From<TestEntity>().WhereAll();

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            var result = results.Results[0];
            result.Id.Should().Be(entity.Id);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(true);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(1);
            result.ALongValue.Should().Be(2);
            result.ADoubleValue.Should().Be(0.1);
            result.AStringValue.Should().Be("astringvalue");
            result.ADateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ADateTimeOffsetUtcValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.AComplexNonValueObjectValue.ToJson().Should()
                .Be(new ComplexNonValueObject {APropertyValue = "avalue"}.ToJson());
            result.AComplexValueObjectValue.Should().Be(ComplexValueObject.Create("avalue", 25, true));
        }

        [TestMethod]
        public void WhenQueryAndSelect_ThenReturnsResultWithOnlySelectedPropertiesPopulated()
        {
            var entity = new TestEntity
            {
                ABinaryValue = new byte[] {0x01},
                ABooleanValue = true,
                ADoubleValue = 0.1,
                AGuidValue = Guid.Empty,
                AIntValue = 1,
                ALongValue = 2,
                AStringValue = "astringvalue",
                ADateTimeUtcValue = DateTime.Today.ToUniversalTime(),
                ADateTimeOffsetUtcValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                AComplexNonValueObjectValue = new ComplexNonValueObject
                {
                    APropertyValue = "avalue"
                },
                AComplexValueObjectValue = ComplexValueObject.Create("avalue", 25, true)
            };

            this.storage.Add(entity);
            var query = Query.From<TestEntity>().WhereAll()
                .Select(e => e.ABinaryValue);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            var result = results.Results[0];
            result.Id.Should().Be(entity.Id);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(false);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(0);
            result.ALongValue.Should().Be(0);
            result.ADoubleValue.Should().Be(0);
            result.AStringValue.Should().Be(null);
            result.ADateTimeUtcValue.Should().Be(DateTime.MinValue);
            result.ADateTimeOffsetUtcValue.Should().Be(DateTimeOffset.MinValue);
            result.AComplexNonValueObjectValue.Should().Be(null);
            result.AComplexValueObjectValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinOnOtherCollection_ThenReturnsOnlyMatchedResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue3"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinAndOtherCollectionNotExists_ThenReturnsAllPrimaryResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinOnOtherCollection_ThenReturnsAllPrimaryResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var entity2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var entity3 = this.storage.Add(new TestEntity {AStringValue = "avalue3"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue5"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(3);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[1].Id.Should().Be(entity2.Id);
            results.Results[2].Id.Should().Be(entity3.Id);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnOtherCollection_ThenReturnsAggregatedResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7});
            this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1", AIntValue = 9});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue3"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[0].AIntValue.Should().Be(9);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinAndOtherCollectionNotExists_ThenReturnsUnAggregatedResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[0].AIntValue.Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinOnOtherCollection_ThenReturnsPartiallyAggregatedResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7});
            var entity2 = this.storage.Add(new TestEntity {AStringValue = "avalue2", AIntValue = 7});
            var entity3 = this.storage.Add(new TestEntity {AStringValue = "avalue3", AIntValue = 7});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1", AIntValue = 9});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue5"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(3);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[0].AIntValue.Should().Be(9);
            results.Results[1].Id.Should().Be(entity2.Id);
            results.Results[1].AIntValue.Should().Be(7);
            results.Results[2].Id.Should().Be(entity3.Id);
            results.Results[2].AIntValue.Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnMultipleOtherCollections_ThenReturnsAggregatedResults()
        {
            var entity1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7, ALongValue = 7});
            this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1", AIntValue = 9});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue3"});
            this.secondJoiningStorage.Add(new SecondJoiningTestEntity
                {AStringValue = "avalue1", AIntValue = 9, ALongValue = 8});
            this.secondJoiningStorage.Add(new SecondJoiningTestEntity {AStringValue = "avalue3"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .AndJoin<SecondJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue)
                .SelectFromJoin<SecondJoiningTestEntity, long>(e => e.ALongValue, je => je.ALongValue);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(entity1.Id);
            results.Results[0].AIntValue.Should().Be(9);
            results.Results[0].ALongValue.Should().Be(8);
        }

        [TestMethod]
        public void WhenQueryAndNoOrderBy_ThenReturnsResultsSortedByDateCreatedAscending()
        {
            var entities = CreateMultipleEntities(100);

            var query = Query.From<TestEntity>()
                .WhereAll();

            var results = this.storage.Query(query);

            VerifyOrderedResults(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndOrderByOnUnSelectedField_ThenReturnsResultsSortedByDateCreatedAscending()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{100 - counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Select(e => e.Id)
                .OrderBy(e => e.AStringValue);

            var results = this.storage.Query(query);

            VerifyOrderedResults(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndOrderByProperty_ThenReturnsResultsSortedByPropertyAscending()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{100 - counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .OrderBy(e => e.AStringValue);

            var results = this.storage.Query(query);

            VerifyOrderedResultsInReverse(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndOrderByPropertyDescending_ThenReturnsResultsSortedByPropertyDescending()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .OrderBy(e => e.AStringValue, OrderDirection.Descending);

            var results = this.storage.Query(query);

            VerifyOrderedResultsInReverse(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndNoTake_ThenReturnsAllResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll();

            var results = this.storage.Query(query);

            VerifyOrderedResults(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndZeroTake_ThenReturnsNoResults()
        {
            CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Take(0);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndTakeLessThanAvailable_ThenReturnsAsManyResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Take(10);

            var results = this.storage.Query(query);

            VerifyOrderedResults(results, entities, 0, 10);
        }

        [TestMethod]
        public void WhenQueryAndTakeMoreThanAvailable_ThenReturnsAllResults()
        {
            var entities =
                CreateMultipleEntities(10, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Take(100);

            var results = this.storage.Query(query);

            VerifyOrderedResults(results, entities, 0, 10);
        }

        [TestMethod]
        public void WhenQueryAndTakeAndOrderByPropertyDescending_ThenReturnsAsManyResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .OrderBy(e => e.AStringValue, OrderDirection.Descending)
                .Take(10);

            var results = this.storage.Query(query);

            VerifyOrderedResultsInReverse(results, entities, 0, 10);
        }

        [TestMethod]
        public void WhenQueryAndTakeAndOrderByDescending_ThenReturnsFirstPageOfResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .OrderBy(e => e.AStringValue, OrderDirection.Descending)
                .Take(10);

            var results = this.storage.Query(query);

            VerifyOrderedResultsInReverse(results, entities, 0, 10);
        }

        [TestMethod]
        public void WhenQueryAndTakeAndSkipAndOrderBy_ThenReturnsNextPageOfResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .OrderBy(e => e.AStringValue)
                .Skip(10)
                .Take(10);

            var results = this.storage.Query(query);

            VerifyOrderedResultsInReverse(results, entities, 10, 10);
        }

        [TestMethod]
        public void WhenQueryAndTakeAndSkipAllAvailable_ThenReturnsNoResults()
        {
            CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .OrderBy(e => e.AStringValue)
                .Skip(100)
                .Take(10);

            var results = this.storage.Query(query);

            results.Results.Count.Should().Be(0);
        }

        private List<Identifier> CreateMultipleEntities(int count, Action<int, TestEntity> factory = null)
        {
            static void IntroduceTimeDelayForSortingDates()
            {
                Thread.Sleep(20);
            }

            var createdIdentifiers = new List<Identifier>();
            Repeat.Times(counter =>
            {
                var entity = new TestEntity();
                factory?.Invoke(counter, entity);
                this.storage.Add(entity);
                createdIdentifiers.Add(entity.Id);
                IntroduceTimeDelayForSortingDates();
            }, count);

            return createdIdentifiers;
        }

        private static void VerifyOrderedResultsInReverse(QueryResults<TestEntity> results, List<Identifier> entities,
            int? offset = null, int? limit = null)
        {
            entities.Reverse();
            VerifyOrderedResults(results, entities, offset, limit);
        }

        private static void VerifyOrderedResults(QueryResults<TestEntity> results, IReadOnlyList<Identifier> entities,
            int? offset = null, int? limit = null)
        {
            var expectedResultCount = limit ?? entities.Count;
            results.Results.Count.Should().Be(expectedResultCount);

            var resultIndex = 0;
            var entityCount = 0;
            results.Results.ForEach(result =>
            {
                if (limit.HasValue && entityCount >= limit.Value)
                {
                    return;
                }

                if (!offset.HasValue || resultIndex >= offset)
                {
                    var createdIdentifier = entities[resultIndex];

                    result.Id.Should().Be(createdIdentifier,
                        $"Result at ({resultIndex}) should have been: {createdIdentifier}");

                    entityCount++;
                }

                resultIndex++;
            });
        }
    }
}