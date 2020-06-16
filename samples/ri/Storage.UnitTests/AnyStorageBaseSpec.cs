using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    public abstract class AnyStorageBaseSpec

    {
        private IStorage<TestEntity> storage;

        [TestInitialize]
        public void Initialize()
        {
            this.storage = GetStorage();
            this.storage.DestroyAll();
        }

        protected abstract IStorage<TestEntity> GetStorage();

        [TestMethod, TestCategory("Unit")]
        public void WhenAddAndEntityNotExists_ThenAddsNew()
        {
            Assert.AreEqual(0, this.storage.Count());

            this.storage.Add(new TestEntity());

            Assert.AreEqual(1, this.storage.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            var id = this.storage.Add(new TestEntity());

            this.storage.Delete(id, false);

            Assert.AreEqual(0, this.storage.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenDeleteAndEntityNotExists_ThenReturns()
        {
            this.storage.Delete("anid", false);

            Assert.AreEqual(0, this.storage.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenDeleteAndIdIsEmpty_ThenThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                this.storage.Delete(null, false));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var entity = this.storage.Get("anid");

            Assert.IsNull(entity);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenGetAndExists_ThenReturnsEntity()
        {
            var entity = new TestEntity();
            var id = this.storage.Add(entity);

            var get = this.storage.Get(id);

            Assert.AreEqual(entity.ToJson(), get.ToJson());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenGetAndIdIsEmpty_ThenThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                this.storage.Get(null));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenUpdateAndExists_ThenReturnsUpdated()
        {
            var entity = new TestEntity();
            this.storage.Add(entity);

            entity.AStringValue = "updated";
            var updated = this.storage.Update(entity, false);

            Assert.AreEqual("updated", updated.AStringValue);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            var entity = new TestEntity("anid")
            {
                AStringValue = "updated"
            };

            Assert.ThrowsException<EntityNotExistsException>(() =>
                this.storage.Update(entity, false));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenUpdateAndEmptyId_ThenThrows()
        {
            var entity = new TestEntity();

            Assert.ThrowsException<EntityNotIdentifiedException>(() =>
                this.storage.Update(entity, false));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCountAndEmpty_ThenReturnsZero()
        {
            var count = this.storage.Count();

            Assert.AreEqual(0, count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenCountAndNotEmpty_ThenReturnsCount()
        {
            this.storage.Add(new TestEntity());
            this.storage.Add(new TestEntity());

            var count = this.storage.Count();

            Assert.AreEqual(2, count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndQueryIsNull_ThenThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                this.storage.Query(null, null));
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, id2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AStringValue = "avalue1"});
            var id2 = this.storage.Add(new TestEntity {AStringValue = "avalue2"});
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTime1 = DateTimeOffset.UtcNow;
            var dateTime2 = DateTimeOffset.UtcNow.AddDays(1);
            this.storage.Add(new TestEntity {ADateTimeOffsetValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeOffsetValue = dateTime2});
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.storage.Add(new TestEntity {ADateTimeValue = dateTime1});
            var id2 = this.storage.Add(new TestEntity {ADateTimeValue = dateTime2});
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ABooleanValue = false});
            var id2 = this.storage.Add(new TestEntity {ABooleanValue = true});
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {AIntValue = 1});
            var id2 = this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var id1 = this.storage.Add(new TestEntity {AIntValue = 1});
            this.storage.Add(new TestEntity {AIntValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ALongValue = 1});
            var id2 = this.storage.Add(new TestEntity {ALongValue = 2});
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.storage.Add(new TestEntity {ADoubleValue = 1.0});
            var id2 = this.storage.Add(new TestEntity {ADoubleValue = 2.0});
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id2, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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

        [TestMethod, TestCategory("Unit")]
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
    }

    public class TestEntity : IKeyedEntity
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
        public DateTime ADateTimeValue { get; set; }
        public DateTimeOffset ADateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
        public double ADoubleValue { get; set; }
        public Guid AGuidValue { get; set; }
        public int AIntValue { get; set; }
        public long ALongValue { get; set; }
        public byte[] ABinaryValue { get; set; }
        public ComplexType AComplexTypeValue { get; set; }

        public string Id { get; set; }

        public string EntityName => null;
    }

    public class ComplexType
    {
        public string APropertyValue { get; set; }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
    }
}