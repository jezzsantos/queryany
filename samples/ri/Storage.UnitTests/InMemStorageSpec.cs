using System;
using CarsApi.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using Storage.Interfaces;

namespace Storage.UnitTests
{
    [TestClass]
    public class InMemStorageSpec
    {
        private TestInMemStorage storage;


        [TestInitialize]
        public void Initialize()
        {
            this.storage = new TestInMemStorage();
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAddAndNoIdentifier_ThenThrows()
        {
            Assert.ThrowsException<EntityNotIdentifiedException>(() =>
            this.storage.Add(new TestEntity(null)));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAddAndEntityNotExists_ThenAddsNew()
        {
            Assert.AreEqual(0, this.storage.Count());

            this.storage.Add(new TestEntity("anid"));

            Assert.AreEqual(1, this.storage.Count());
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenAddAndEntityExists_ThenThrows()
        {
            this.storage.Add(new TestEntity("anid"));

            Assert.ThrowsException<EntityAlreadyExistsException>(() =>
            this.storage.Add(new TestEntity("anid")));
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            this.storage.Add(new TestEntity("anid"));

            this.storage.Delete("anid", false);

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
            var entity = new TestEntity("anid");
            this.storage.Add(entity);

            var get = this.storage.Get("anid");

            Assert.AreEqual(entity, get);
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
            var entity = new TestEntity("anid");
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
            var entity = new TestEntity(null);

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
            this.storage.Add(new TestEntity("anid1"));
            this.storage.Add(new TestEntity("anid2"));

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
        public void WhenQueryAndNoExpressions_ThenReturnsEmptyResults()
        {
            var query = Query.Empty();
            this.storage.Add(new TestEntity("anid1")
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndEmpty_ThenReturnsEmptyResults()
        {
            var query = Query.Create<TestEntity>(e => e.AProperty, QueryOperator.EQ, "avalue");

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndNoMatch_ThenReturnsEmptyResults()
        {
            var query = Query.Create<TestEntity>(e => e.AProperty, QueryOperator.EQ, "anothervalue");
            this.storage.Add(new TestEntity("anid1")
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(0, results.Results.Count);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndMatchOne_ThenReturnsResult()
        {
            var query = Query.Create<TestEntity>(e => e.AProperty, QueryOperator.EQ, "avalue");
            this.storage.Add(new TestEntity("anid1")
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(1, results.Results.Count);
            Assert.AreEqual("anid1", results.Results[0].Id);
        }

        [TestMethod, TestCategory("Unit")]
        public void WhenQueryAndMatchMaany_ThenReturnsResults()
        {
            var query = Query.Create<TestEntity>(e => e.AProperty, QueryOperator.EQ, "avalue");
            this.storage.Add(new TestEntity("anid1")
            {
                AProperty = "avalue"
            });
            this.storage.Add(new TestEntity("anid2")
            {
                AProperty = "avalue"
            });

            var results = this.storage.Query(query, null);

            Assert.AreEqual(2, results.Results.Count);
            Assert.AreEqual("anid1", results.Results[0].Id);
            Assert.AreEqual("anid2", results.Results[1].Id);
        }

        //TODO: for limits, sort, etc
    }

    public class TestInMemStorage : InMemStorage<TestEntity>
    {

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

        public string Id { get; set; }
        public string AProperty { get; set; }
    }
}
