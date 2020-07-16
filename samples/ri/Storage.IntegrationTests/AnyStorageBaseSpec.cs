using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using Services.Interfaces;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    public abstract class AnyStorageBaseSpec

    {
        private IStorage<FirstJoiningTestEntity> firstJoiningStorage;
        private IStorage<SecondJoiningTestEntity> secondJoiningStorage;
        private IStorage<TestEntity> storage;

        protected ILogger Logger { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            Logger = new Logger<AnyStorageBaseSpec>(new NullLoggerFactory());
            this.storage = GetStore(new TestEntity().EntityName, TestEntity.GetFactory());
            this.storage.DestroyAll();
            this.firstJoiningStorage =
                GetStore(new FirstJoiningTestEntity().EntityName, FirstJoiningTestEntity.GetFactory());
            this.firstJoiningStorage.DestroyAll();
            this.secondJoiningStorage = GetStore(new SecondJoiningTestEntity().EntityName,
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

            var id = this.storage.Add(new TestEntity());

            this.storage.Count().Should().Be(1);

            var added = this.storage.Get(id);
            added.Id.Should().Be(id);
            added.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            added.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            added.CreatedAtUtc.Should().Be(added.LastModifiedAtUtc);
        }

        [TestMethod]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            var id = this.storage.Add(new TestEntity());

            this.storage.Delete(id);

            this.storage.Count().Should().Be(0);
        }

        [TestMethod]
        public void WhenDeleteAndEntityNotExists_ThenReturns()
        {
            this.storage.Delete(Identifier.Create("anid"));

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
            var entity = this.storage.Get(Identifier.Create("anid"));

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
                AComplexNonValueTypeValue = new ComplexNonValueType
                {
                    APropertyValue = "avalue"
                },
                AComplexValueTypeValue = ComplexValueType.Create("avalue", 25, true)
            };

            var id = this.storage.Add(entity);

            var result = this.storage.Get(id);

            result.Id.Should().Be(id);
            result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(true);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(1);
            result.ALongValue.Should().Be(2);
            result.ADoubleValue.Should().Be(0.1);
            result.AStringValue.Should().Be("astringvalue");
            result.ADateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ADateTimeOffsetUtcValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.AComplexNonValueTypeValue.ToString().Should()
                .Be(new ComplexNonValueType {APropertyValue = "avalue"}.ToString());
            result.AComplexValueTypeValue.Should().Be(ComplexValueType.Create("avalue", 25, true));
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
            var id = this.storage.Add(entity);

            entity.AStringValue = "updated";
            var updated = this.storage.Update(entity);

            updated.Id.Should().Be(id);
            updated.AStringValue.Should().Be("updated");
            updated.LastModifiedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            updated.LastModifiedAtUtc.Should().BeAfter(updated.CreatedAtUtc,
                $"{nameof(IModifiableEntity.LastModifiedAtUtc)} ({updated.LastModifiedAtUtc:O}) is not after {nameof(IModifiableEntity.CreatedAtUtc)} ({updated.CreatedAtUtc:O})");
        }

        [TestMethod]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            var entity = new TestEntity(Identifier.Create("anid"))
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
            this.storage.Invoking(x => x.Query(null, null))
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

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndWhereAll_ThenReturnsAllResults()
        {
            var query = Query.From<TestEntity>()
                .WhereAll();
            var id = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id);
        }

        [TestMethod]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.storage.Query(query, null);

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

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndMatchOne_ThenReturnsResult()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var id = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id);
        }

        [TestMethod]
        public void WhenQueryAndMatchMany_ThenReturnsResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var id1 = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });
            var id2 = this.storage.Add(new TestEntity
            {
                AStringValue = "avalue"
            });

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(id1);
            results.Results[1].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, id2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForNullStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForNotNullStringValue_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTime1 = DateTimeOffset.UtcNow;
            var dateTime2 = DateTimeOffset.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeOffsetUtcValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeOffsetUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueGreaterThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThan, dateTime1);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var id1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThanEqualTo, dateTime1);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(id1);
            results.Results[1].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueLessThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var id1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThan, dateTime2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var id1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThanEqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(id1);
            results.Results[1].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForDateTimeValueNotEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var id1 = this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime1});
            this.storage.Add(new TestEntity {ADateTimeUtcValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.NotEqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ABooleanValue = false});
            var id2 = this.storage.Add(new TestEntity {ABooleanValue = true});
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(id1);
            results.Results[1].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(2);
            results.Results[0].Id.Should().Be(id1);
            results.Results[1].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ALongValue = 1});
            var id2 = this.storage.Add(new TestEntity {ALongValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ADoubleValue = 1.0});
            var id2 = this.storage.Add(new TestEntity {ADoubleValue = 2.0});
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForGuidValue_ThenReturnsResult()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.storage.Add(new TestEntity {AGuidValue = guid1});
            var id2 = this.storage.Add(new TestEntity {AGuidValue = guid2});
            var query = Query.From<TestEntity>().Where(e => e.AGuidValue, ConditionOperator.EqualTo, guid2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForBinaryValue_ThenReturnsResult()
        {
            var binary1 = new byte[] {0x01};
            var binary2 = new byte[] {0x01, 0x02};
            this.storage.Add(new TestEntity {ABinaryValue = binary1});
            var id2 = this.storage.Add(new TestEntity {ABinaryValue = binary2});
            var query = Query.From<TestEntity>().Where(e => e.ABinaryValue, ConditionOperator.EqualTo, binary2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
        }

        [TestMethod]
        public void WhenQueryForComplexNonValueTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueType {APropertyValue = "avalue1"};
            var complex2 = new ComplexNonValueType {APropertyValue = "avalue2"};
            this.storage.Add(new TestEntity {AComplexNonValueTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexNonValueTypeValue = complex2});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueTypeValue, ConditionOperator.EqualTo, complex2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
            results.Results[0].AComplexNonValueTypeValue.ToString().Should().Be(complex2.ToString());
        }

        [TestMethod]
        public void WhenQueryForNullComplexNonValueTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueType {APropertyValue = "avalue1"};
            this.storage.Add(new TestEntity {AComplexNonValueTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexNonValueTypeValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueTypeValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
            results.Results[0].AComplexNonValueTypeValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexNonValueTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueType {APropertyValue = "avalue1"};
            var id1 = this.storage.Add(new TestEntity {AComplexNonValueTypeValue = complex1});
            this.storage.Add(new TestEntity {AComplexNonValueTypeValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueTypeValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
            results.Results[0].AComplexNonValueTypeValue.ToString().Should().Be(complex1.ToString());
        }


        [TestMethod]
        public void WhenQueryForComplexValueTypeValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueType.Create("avalue1", 25, true);
            var complex2 = ComplexValueType.Create("avalue2", 50, false);
            this.storage.Add(new TestEntity {AComplexValueTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexValueTypeValue = complex2});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueTypeValue, ConditionOperator.EqualTo, complex2);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
            results.Results[0].AComplexValueTypeValue.Should().Be(complex2);
        }

        [TestMethod]
        public void WhenQueryForNullComplexValueTypeValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueType.Create("avalue1", 25, true);
            this.storage.Add(new TestEntity {AComplexValueTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexValueTypeValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueTypeValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id2);
            results.Results[0].AComplexValueTypeValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexValueTypeValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueType.Create("avalue1", 25, true);
            var id1 = this.storage.Add(new TestEntity {AComplexValueTypeValue = complex1});
            this.storage.Add(new TestEntity {AComplexValueTypeValue = null});
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueTypeValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
            results.Results[0].AComplexValueTypeValue.Should().Be(complex1);
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
                AComplexNonValueTypeValue = new ComplexNonValueType
                {
                    APropertyValue = "avalue"
                },
                AComplexValueTypeValue = ComplexValueType.Create("avalue", 25, true)
            };

            var id = this.storage.Add(entity);
            var query = Query.From<TestEntity>().WhereAll();

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            var result = results.Results[0];
            result.Id.Should().Be(id);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(true);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(1);
            result.ALongValue.Should().Be(2);
            result.ADoubleValue.Should().Be(0.1);
            result.AStringValue.Should().Be("astringvalue");
            result.ADateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ADateTimeOffsetUtcValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.AComplexNonValueTypeValue.ToJson().Should()
                .Be(new ComplexNonValueType {APropertyValue = "avalue"}.ToJson());
            result.AComplexValueTypeValue.Should().Be(ComplexValueType.Create("avalue", 25, true));
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
                AComplexNonValueTypeValue = new ComplexNonValueType
                {
                    APropertyValue = "avalue"
                },
                AComplexValueTypeValue = ComplexValueType.Create("avalue", 25, true)
            };

            var id = this.storage.Add(entity);
            var query = Query.From<TestEntity>().WhereAll()
                .Select(e => e.ABinaryValue);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            var result = results.Results[0];
            result.Id.Should().Be(id);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(false);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(0);
            result.ALongValue.Should().Be(0);
            result.ADoubleValue.Should().Be(0);
            result.AStringValue.Should().Be(null);
            result.ADateTimeUtcValue.Should().Be(DateTime.MinValue);
            result.ADateTimeOffsetUtcValue.Should().Be(DateTimeOffset.MinValue);
            result.AComplexNonValueTypeValue.Should().Be(null);
            result.AComplexValueTypeValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinOnOtherCollection_ThenReturnsOnlyMatchedResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue3"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinAndOtherCollectionNotExists_ThenReturnsAllPrimaryResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinOnOtherCollection_ThenReturnsAllPrimaryResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var id3 = this.storage.Add(new TestEntity {AStringValue = "avalue3"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue5"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(3);
            results.Results[0].Id.Should().Be(id1);
            results.Results[1].Id.Should().Be(id2);
            results.Results[2].Id.Should().Be(id3);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnOtherCollection_ThenReturnsAggregatedResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7});
            this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1", AIntValue = 9});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue3"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
            results.Results[0].AIntValue.Should().Be(9);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinAndOtherCollectionNotExists_ThenReturnsUnAggregatedResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
            results.Results[0].AIntValue.Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinOnOtherCollection_ThenReturnsPartiallyAggregatedResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2", AIntValue = 7});
            var id3 = this.storage.Add(new TestEntity {AStringValue = "avalue3", AIntValue = 7});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue1", AIntValue = 9});
            this.firstJoiningStorage.Add(new FirstJoiningTestEntity {AStringValue = "avalue5"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(3);
            results.Results[0].Id.Should().Be(id1);
            results.Results[0].AIntValue.Should().Be(9);
            results.Results[1].Id.Should().Be(id2);
            results.Results[1].AIntValue.Should().Be(7);
            results.Results[2].Id.Should().Be(id3);
            results.Results[2].AIntValue.Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnMultipleOtherCollections_ThenReturnsAggregatedResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1", AIntValue = 7, ALongValue = 7});
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

            var results = this.storage.Query(query, null);

            results.Results.Count.Should().Be(1);
            results.Results[0].Id.Should().Be(id1);
            results.Results[0].AIntValue.Should().Be(9);
            results.Results[0].ALongValue.Should().Be(8);
        }
    }
}