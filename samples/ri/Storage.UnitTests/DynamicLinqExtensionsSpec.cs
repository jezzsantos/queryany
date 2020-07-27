using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Services.Interfaces.Entities;

namespace Storage.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class DynamicLinqExtensionsSpec
    {
        [TestMethod]
        public void WhenDistinctByWithNullPropertyName_ThenReturnsSource()
        {
            var source = new List<TestEntity>().AsQueryable();

            var result = source.DistinctBy(null);

            result.Should().BeSameAs(source);
        }

        [TestMethod]
        public void WhenDistinctByWithEmptySource_ThenReturnsSource()
        {
            var source = new List<TestEntity>().AsQueryable();

            var result = source.DistinctBy(nameof(TestEntity.AStringValue));

            result.Should().BeSameAs(source);
        }

        [TestMethod]
        public void WhenDistinctByWithUnknownPropertyName_ThenReturnsSource()
        {
            var source = new List<TestEntity>
            {
                new TestEntity {Id = Identifier.Create("anid1"), AStringValue = "avalue"},
                new TestEntity {Id = Identifier.Create("anid2"), AStringValue = "avalue"},
                new TestEntity {Id = Identifier.Create("anid3"), AStringValue = "avalue"}
            }.AsQueryable();

            var result = source.DistinctBy("anotherpropertyname");

            result.Should().BeSameAs(source);
        }

        [TestMethod]
        public void WhenDistinctByWithDuplicateNullValues_ThenReturnsFirstValue()
        {
            var source = new List<TestEntity>
            {
                new TestEntity {Id = Identifier.Create("anid1"), AStringValue = null},
                new TestEntity {Id = Identifier.Create("anid2"), AStringValue = null}
            }.AsQueryable();

            var result = source.DistinctBy(nameof(TestEntity.AStringValue))
                .ToList();

            result.Count.Should().Be(1);
            result[0].Id.Get().Should().Be("anid1");
            result[0].AStringValue.Should().BeNull();
        }

        [TestMethod]
        public void WhenDistinctByWithDuplicateValues_ThenReturnsFirstValue()
        {
            var source = new List<TestEntity>
            {
                new TestEntity {Id = Identifier.Create("anid1"), AStringValue = "avalue"},
                new TestEntity {Id = Identifier.Create("anid2"), AStringValue = "avalue"}
            }.AsQueryable();

            var result = source.DistinctBy(nameof(TestEntity.AStringValue))
                .ToList();

            result.Count.Should().Be(1);
            result[0].Id.Get().Should().Be("anid1");
            result[0].AStringValue.Should().Be("avalue");
        }

        [TestMethod]
        public void WhenDistinctByWithMultipleDuplicateValues_ThenReturnsFirstValues()
        {
            var source = new List<TestEntity>
            {
                new TestEntity {Id = Identifier.Create("anid1"), AStringValue = "avalue1"},
                new TestEntity {Id = Identifier.Create("anid2"), AStringValue = "avalue1"},
                new TestEntity {Id = Identifier.Create("anid3"), AStringValue = "avalue2"},
                new TestEntity {Id = Identifier.Create("anid4"), AStringValue = "avalue2"},
                new TestEntity {Id = Identifier.Create("anid5"), AStringValue = null},
                new TestEntity {Id = Identifier.Create("anid6"), AStringValue = null}
            }.AsQueryable();

            var result = source.DistinctBy(nameof(TestEntity.AStringValue))
                .ToList();

            result.Count.Should().Be(3);
            result[0].Id.Get().Should().Be("anid1");
            result[1].Id.Get().Should().Be("anid3");
            result[2].Id.Get().Should().Be("anid5");
        }
    }
}