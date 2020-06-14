using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    public abstract class StorageSpecBase
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

            entity.AProperty = "updated";
            var updated = this.storage.Update(entity, false);

            Assert.AreEqual("updated", updated.AProperty);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenUpdateAndNotExists_ThenThrows()
        {
            var entity = new TestEntity("anid")
            {
                AProperty = "updated"
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
                AProperty = "avalue"
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
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AProperty, ConditionOperator.EqualTo, "avalue");

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndNoMatch_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AProperty, ConditionOperator.EqualTo, "anothervalue");
            this.storage.Add(new TestEntity
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndMatchOne_ThenReturnsResult()
        {
            var query = Query.From<TestEntity>().Where(e => e.AProperty, ConditionOperator.EqualTo, "avalue");
            var id = this.storage.Add(new TestEntity
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual(id, results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndMatchMany_ThenReturnsResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AProperty, ConditionOperator.EqualTo, "avalue");
            var id1 = this.storage.Add(new TestEntity
            {
                AProperty = "avalue"
            });
            var id2 = this.storage.Add(new TestEntity
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual(id1, results.Results[0].Id);
            Assert.AreEqual(id2, results.Results[1].Id);
        }

        //TODO: for limits, sort, etc
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

        public string AProperty { get; set; }

        public string Id { get; set; }

        public string EntityName => null;
    }
}