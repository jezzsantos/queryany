using System;
using System.Linq;
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
        private static readonly IAssertion Assert = new Assertion();
        private IStorage<FirstJoiningTestEntity> firstJoiningStorage;
        private IStorage<SecondJoiningTestEntity> secondJoiningStorage;
        private IStorage<TestEntity> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.storage = GetStore<TestEntity>(new TestEntity().EntityName);
            this.storage.DestroyAll();
            this.firstJoiningStorage = GetStore<FirstJoiningTestEntity>(new FirstJoiningTestEntity().EntityName);
            this.firstJoiningStorage.DestroyAll();
            this.secondJoiningStorage = GetStore<SecondJoiningTestEntity>(new SecondJoiningTestEntity().EntityName);
            this.secondJoiningStorage.DestroyAll();
        }

        protected abstract IStorage<TEntity> GetStore<TEntity>(string containerName)
            where TEntity : IPersistableEntity, new();

        [TestMethod]
        public void WhenAddAndEntityNotExists_ThenAddsNew()
        {
            Assert.Equal(0, this.storage.Count());

            var id = this.storage.Add(new TestEntity());

            Assert.Equal(1, this.storage.Count());

            var added = this.storage.Get(id);
            Assert.Equal(id, added.Id);
            Assert.Near(DateTime.UtcNow, added.CreatedAtUtc);
            Assert.Near(DateTime.UtcNow, added.LastModifiedAtUtc);
            Assert.True(added.LastModifiedAtUtc == added.CreatedAtUtc);
        }

        [TestMethod]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            var id = this.storage.Add(new TestEntity());

            this.storage.Delete(id, false);

            Assert.Equal(0, this.storage.Count());
        }

        [TestMethod]
        public void WhenDeleteAndEntityNotExists_ThenReturns()
        {
            this.storage.Delete(Identifier.Create("anid"), false);

            Assert.Equal(0, this.storage.Count());
        }

        [TestMethod]
        public void WhenDeleteAndIdIsEmpty_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                this.storage.Delete(null, false));
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var entity = this.storage.Get(Identifier.Create("anid"));

            Assert.Null(entity);
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
                AComplexTypeValue = new ComplexType
                {
                    APropertyValue = "avalue"
                }
            };

            var id = this.storage.Add(entity);

            var result = this.storage.Get(id);

            Assert.Equal(id, result.Id);
            Assert.Near(DateTime.UtcNow, result.CreatedAtUtc);
            Assert.Near(DateTime.UtcNow, result.LastModifiedAtUtc);
            Assert.True(result.ABinaryValue.SequenceEqual(new byte[] {0x01}));
            Assert.Equal(true, result.ABooleanValue);
            Assert.Equal(Guid.Empty, result.AGuidValue);
            Assert.Equal(1, result.AIntValue);
            Assert.Equal(2, result.ALongValue);
            Assert.Equal(0.1, result.ADoubleValue);
            Assert.Equal("astringvalue", result.AStringValue);
            Assert.Equal(DateTime.Today.ToUniversalTime(), result.ADateTimeUtcValue);
            Assert.Equal(DateTimeOffset.UnixEpoch.ToUniversalTime(), result.ADateTimeOffsetUtcValue);
            Assert.Equal(new ComplexType {APropertyValue = "avalue"}.ToJson(), result.AComplexTypeValue.ToJson());
        }

        [TestMethod]
        public void WhenGetAndIdIsEmpty_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                this.storage.Get(null));
        }

        [TestMethod]
        public void WhenUpdateAndExists_ThenReturnsUpdated()
        {
            var entity = new TestEntity();
            var id = this.storage.Add(entity);

            entity.AStringValue = "updated";
            var updated = this.storage.Update(entity, false);

            Assert.Equal(id, updated.Id);
            Assert.Equal("updated", updated.AStringValue);
            Assert.Near(DateTime.UtcNow, updated.LastModifiedAtUtc);
            Assert.True(updated.LastModifiedAtUtc > updated.CreatedAtUtc,
                $"{nameof(IModifiableEntity.LastModifiedAtUtc)} ({updated.LastModifiedAtUtc:O}) is not after {nameof(IModifiableEntity.CreatedAtUtc)} ({updated.CreatedAtUtc:O})");
        }

        [TestMethod]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            var entity = new TestEntity(Identifier.Create("anid"))
            {
                AStringValue = "updated"
            };

            Assert.Throws<ResourceNotFoundException>(() =>
                this.storage.Update(entity, false));
        }

        [TestMethod]
        public void WhenUpdateAndEmptyId_ThenThrows()
        {
            var entity = new TestEntity();

            Assert.Throws<ResourceNotFoundException>(() =>
                this.storage.Update(entity, false));
        }

        [TestMethod]
        public void WhenCountAndEmpty_ThenReturnsZero()
        {
            var count = this.storage.Count();

            Assert.Equal(0, count);
        }

        [TestMethod]
        public void WhenCountAndNotEmpty_ThenReturnsCount()
        {
            this.storage.Add(new TestEntity());
            this.storage.Add(new TestEntity());

            var count = this.storage.Count();

            Assert.Equal(2, count);
        }

        [TestMethod]
        public void WhenQueryAndQueryIsNull_ThenThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                this.storage.Query(null, null));
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

            Assert.Equal(0, results.Results.Count);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.storage.Query(query, null);

            Assert.Equal(0, results.Results.Count);
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

            Assert.Equal(0, results.Results.Count);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id, results.Results[0].Id);
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

            Assert.Equal(2, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(id2, results.Results[1].Id);
        }

        [TestMethod]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, id2);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNullStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNotNullStringValue_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
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

            Assert.Equal(2, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(id2, results.Results[1].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
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

            Assert.Equal(2, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(id2, results.Results[1].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ABooleanValue = false});
            var id2 = this.storage.Add(new TestEntity {ABooleanValue = true});
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.storage.Query(query, null);

            Assert.Equal(2, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(id2, results.Results[1].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.Equal(2, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(id2, results.Results[1].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ALongValue = 1});
            var id2 = this.storage.Add(new TestEntity {ALongValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ADoubleValue = 1.0});
            var id2 = this.storage.Add(new TestEntity {ADoubleValue = 2.0});
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForComplexTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexType {APropertyValue = "avalue1"};
            var complex2 = new ComplexType {APropertyValue = "avalue2"};
            this.storage.Add(new TestEntity {AComplexTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexTypeValue = complex2});
            var query = Query.From<TestEntity>().Where(e => e.AComplexTypeValue, ConditionOperator.EqualTo, complex2);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNullComplexTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexType {APropertyValue = "avalue1"};
            this.storage.Add(new TestEntity {AComplexTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexTypeValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AComplexTypeValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexType {APropertyValue = "avalue1"};
            var id1 = this.storage.Add(new TestEntity {AComplexTypeValue = complex1});
            this.storage.Add(new TestEntity {AComplexTypeValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AComplexTypeValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
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
                AComplexTypeValue = new ComplexType
                {
                    APropertyValue = "avalue"
                }
            };

            var id = this.storage.Add(entity);
            var query = Query.From<TestEntity>().WhereAll();

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            var result = results.Results[0];
            Assert.Equal(id, result.Id);
            Assert.True(result.ABinaryValue.SequenceEqual(new byte[] {0x01}));
            Assert.Equal(true, result.ABooleanValue);
            Assert.Equal(Guid.Empty, result.AGuidValue);
            Assert.Equal(1, result.AIntValue);
            Assert.Equal(2, result.ALongValue);
            Assert.Equal(0.1, result.ADoubleValue);
            Assert.Equal("astringvalue", result.AStringValue);
            Assert.Equal(DateTime.Today.ToUniversalTime(), result.ADateTimeUtcValue);
            Assert.Equal(DateTimeOffset.UnixEpoch.ToUniversalTime(), result.ADateTimeOffsetUtcValue);
            Assert.Equal(new ComplexType {APropertyValue = "avalue"}.ToJson(), result.AComplexTypeValue.ToJson());
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
                AComplexTypeValue = new ComplexType
                {
                    APropertyValue = "avalue"
                }
            };

            var id = this.storage.Add(entity);
            var query = Query.From<TestEntity>().WhereAll()
                .Select(e => e.ABinaryValue);

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            var result = results.Results[0];
            Assert.Equal(id, result.Id);
            Assert.True(result.ABinaryValue.SequenceEqual(new byte[] {0x01}));
            Assert.Equal(false, result.ABooleanValue);
            Assert.Equal(Guid.Empty, result.AGuidValue);
            Assert.Equal(0, result.AIntValue);
            Assert.Equal(0, result.ALongValue);
            Assert.Equal(0, result.ADoubleValue);
            Assert.Equal(null, result.AStringValue);
            Assert.Equal(DateTime.MinValue, result.ADateTimeUtcValue);
            Assert.Equal(DateTimeOffset.MinValue, result.ADateTimeOffsetUtcValue);
            Assert.Equal(null, result.AComplexTypeValue);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.storage.Query(query, null);

            Assert.Equal(0, results.Results.Count);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinAndOtherCollectionNotExists_ThenReturnsAllPrimaryResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.storage.Query(query, null);

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
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

            Assert.Equal(3, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(id2, results.Results[1].Id);
            Assert.Equal(id3, results.Results[2].Id);
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

            Assert.Equal(0, results.Results.Count);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(9, results.Results[0].AIntValue);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(7, results.Results[0].AIntValue);
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

            Assert.Equal(3, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(9, results.Results[0].AIntValue);
            Assert.Equal(id2, results.Results[1].Id);
            Assert.Equal(7, results.Results[1].AIntValue);
            Assert.Equal(id3, results.Results[2].Id);
            Assert.Equal(7, results.Results[2].AIntValue);
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

            Assert.Equal(1, results.Results.Count);
            Assert.Equal(id1, results.Results[0].Id);
            Assert.Equal(9, results.Results[0].AIntValue);
            Assert.Equal(8, results.Results[0].ALongValue);
        }
    }
}