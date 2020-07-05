using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using Services.Interfaces;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    public abstract class AnyStorageBaseSpec

    {
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
            Assert.AreEqual(0, this.storage.Count());

            this.storage.Add(new TestEntity());

            Assert.AreEqual(1, this.storage.Count());
        }

        [TestMethod]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            var id = this.storage.Add(new TestEntity());

            this.storage.Delete(id, false);

            Assert.AreEqual(0, this.storage.Count());
        }

        [TestMethod]
        public void WhenDeleteAndEntityNotExists_ThenReturns()
        {
            this.storage.Delete("anid", false);

            Assert.AreEqual(0, this.storage.Count());
        }

        [TestMethod]
        public void WhenDeleteAndIdIsEmpty_ThenThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                this.storage.Delete(null, false));
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var entity = this.storage.Get("anid");

            Assert.IsNull(entity);
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

            Assert.IsTrue(result.ABinaryValue.SequenceEqual(new byte[] {0x01}));
            Assert.AreEqual(true, result.ABooleanValue);
            Assert.AreEqual(Guid.Empty, result.AGuidValue);
            Assert.AreEqual(1, result.AIntValue);
            Assert.AreEqual(2, result.ALongValue);
            Assert.AreEqual(0.1, result.ADoubleValue);
            Assert.AreEqual("astringvalue", result.AStringValue);
            Assert.AreEqual(DateTime.Today.ToUniversalTime(), result.ADateTimeUtcValue);
            Assert.AreEqual(DateTimeOffset.UnixEpoch.ToUniversalTime(), result.ADateTimeOffsetUtcValue);
            Assert.AreEqual(new ComplexType {APropertyValue = "avalue"}.ToJson(), result.AComplexTypeValue.ToJson());
        }

        [TestMethod]
        public void WhenGetAndIdIsEmpty_ThenThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                this.storage.Get(null));
        }

        [TestMethod]
        public void WhenUpdateAndExists_ThenReturnsUpdated()
        {
            var entity = new TestEntity();
            this.storage.Add(entity);

            entity.AStringValue = "updated";
            var updated = this.storage.Update(entity, false);

            Assert.AreEqual("updated", updated.AStringValue);
        }

        [TestMethod]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            var entity = new TestEntity("anid")
            {
                AStringValue = "updated"
            };

            Assert.ThrowsException<ResourceNotFoundException>(() =>
                this.storage.Update(entity, false));
        }

        [TestMethod]
        public void WhenUpdateAndEmptyId_ThenThrows()
        {
            var entity = new TestEntity();

            Assert.ThrowsException<ResourceNotFoundException>(() =>
                this.storage.Update(entity, false));
        }

        [TestMethod]
        public void WhenCountAndEmpty_ThenReturnsZero()
        {
            var count = this.storage.Count();

            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void WhenCountAndNotEmpty_ThenReturnsCount()
        {
            this.storage.Add(new TestEntity());
            this.storage.Add(new TestEntity());

            var count = this.storage.Count();

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void WhenQueryAndQueryIsNull_ThenThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
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

            Assert.AreEqual(0, results.Results.Count);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
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

            Assert.AreEqual(0, results.Results.Count);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id, results.Results[0].Id);
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

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
        }

        [TestMethod]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, id2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNullStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNotNullStringValue_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            this.storage.Add(new TestEntity {AStringValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
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

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ABooleanValue = false});
            var id2 = this.storage.Add(new TestEntity {ABooleanValue = true});
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ALongValue = 1});
            var id2 = this.storage.Add(new TestEntity {ALongValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ADoubleValue = 1.0});
            var id2 = this.storage.Add(new TestEntity {ADoubleValue = 2.0});
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNullComplexTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexType {APropertyValue = "avalue1"};
            this.storage.Add(new TestEntity {AComplexTypeValue = complex1});
            var id2 = this.storage.Add(new TestEntity {AComplexTypeValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AComplexTypeValue, ConditionOperator.EqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexTypeValue_ThenReturnsResult()
        {
            var complex1 = new ComplexType {APropertyValue = "avalue1"};
            var id1 = this.storage.Add(new TestEntity {AComplexTypeValue = complex1});
            this.storage.Add(new TestEntity {AComplexTypeValue = null});
            var query = Query.From<TestEntity>().Where(e => e.AComplexTypeValue, ConditionOperator.NotEqualTo, null);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
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

            Assert.AreEqual(1, results.Results.Count);
            var result = results.Results[0];
            Assert.AreEqual(id, result.Id);
            Assert.IsTrue(result.ABinaryValue.SequenceEqual(new byte[] {0x01}));
            Assert.AreEqual(true, result.ABooleanValue);
            Assert.AreEqual(Guid.Empty, result.AGuidValue);
            Assert.AreEqual(1, result.AIntValue);
            Assert.AreEqual(2, result.ALongValue);
            Assert.AreEqual(0.1, result.ADoubleValue);
            Assert.AreEqual("astringvalue", result.AStringValue);
            Assert.AreEqual(DateTime.Today.ToUniversalTime(), result.ADateTimeUtcValue);
            Assert.AreEqual(DateTimeOffset.UnixEpoch.ToUniversalTime(), result.ADateTimeOffsetUtcValue);
            Assert.AreEqual(new ComplexType {APropertyValue = "avalue"}.ToJson(), result.AComplexTypeValue.ToJson());
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

            Assert.AreEqual(1, results.Results.Count);
            var result = results.Results[0];
            Assert.AreEqual(id, result.Id);
            Assert.IsTrue(result.ABinaryValue.SequenceEqual(new byte[] {0x01}));
            Assert.AreEqual(false, result.ABooleanValue);
            Assert.AreEqual(Guid.Empty, result.AGuidValue);
            Assert.AreEqual(0, result.AIntValue);
            Assert.AreEqual(0, result.ALongValue);
            Assert.AreEqual(0, result.ADoubleValue);
            Assert.AreEqual(null, result.AStringValue);
            Assert.AreEqual(DateTime.MinValue, result.ADateTimeUtcValue);
            Assert.AreEqual(DateTimeOffset.MinValue, result.ADateTimeOffsetUtcValue);
            Assert.AreEqual(null, result.AComplexTypeValue);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinAndOtherCollectionNotExists_ThenReturnsAllPrimaryResults()
        {
            var id1 = this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
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

            Assert.AreEqual(3, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
            Assert.AreEqual(id3, results.Results[2].Id);
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

            Assert.AreEqual(0, results.Results.Count);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(9, results.Results[0].AIntValue);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(7, results.Results[0].AIntValue);
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

            Assert.AreEqual(3, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(9, results.Results[0].AIntValue);
            Assert.AreEqual(id2, results.Results[1].Id);
            Assert.AreEqual(7, results.Results[1].AIntValue);
            Assert.AreEqual(id3, results.Results[2].Id);
            Assert.AreEqual(7, results.Results[2].AIntValue);
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

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(9, results.Results[0].AIntValue);
            Assert.AreEqual(8, results.Results[0].ALongValue);
        }
    }

    public class TestEntity : IPersistableEntity
    {
        public TestEntity()
        {
        }

        public TestEntity(string id)
        {
            Id = id;
        }

        public string AStringValue { get; set; }
        public bool ABooleanValue { get; set; }
        public DateTime ADateTimeUtcValue { get; set; }
        public DateTimeOffset ADateTimeOffsetUtcValue { get; set; }
        public double ADoubleValue { get; set; }
        public Guid AGuidValue { get; set; }
        public int AIntValue { get; set; }
        public long ALongValue { get; set; }
        public byte[] ABinaryValue { get; set; }
        public ComplexType AComplexTypeValue { get; set; }

        public string Id { get; private set; }

        public void Identify(string id)
        {
            Id = id;
        }

        public string EntityName => "testentities";

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties.FromObjectDictionary<TestEntity>());
        }
    }

    public class FirstJoiningTestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }
        public int AIntValue { get; set; }

        public string EntityName => "firstjoiningtestentities";

        public string Id { get; private set; }

        public void Identify(string id)
        {
            Id = id;
        }

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties.FromObjectDictionary<TestEntity>());
        }
    }

    public class SecondJoiningTestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int AIntValue { get; set; }

        public long ALongValue { get; set; }

        public string EntityName => "secondjoiningtestentities";

        public string Id { get; private set; }

        public void Identify(string id)
        {
            Id = id;
        }

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties.FromObjectDictionary<TestEntity>());
        }
    }

    public class ComplexType
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string APropertyValue { get; set; }

        public override string ToString()
        {
            return this.ToJson();
        }
    }
}