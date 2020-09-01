using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Api.Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Funq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;

namespace Storage.IntegrationTests
{
    public struct RepoInfo
    {
        public IRepository Repository { get; set; }

        public string ContainerName { get; set; }
    }

    public abstract class AnyRepositoryBaseSpec

    {
        private static readonly ILogger Logger = new Logger<AnyRepositoryBaseSpec>(new NullLoggerFactory());
        private static Container container;
        private static IDomainFactory domainFactory;
        private RepoInfo firstJoiningRepo;
        private RepoInfo repo;
        private RepoInfo secondJoiningRepo;

        protected static void InitializeAllTests()
        {
            container = new Container();
            container.AddSingleton(Logger);
            domainFactory = new DomainFactory(new FuncDependencyContainer(container));
            domainFactory.RegisterTypesFromAssemblies(typeof(TestEntity).Assembly);
        }

        [TestInitialize]
        public void Initialize()
        {
            this.repo = GetRepository<TestEntity>();
            this.repo.Repository.DestroyAll(this.repo.ContainerName);
            this.firstJoiningRepo = GetRepository<FirstJoiningTestEntity>();
            this.firstJoiningRepo.Repository.DestroyAll(this.firstJoiningRepo.ContainerName);
            this.secondJoiningRepo = GetRepository<SecondJoiningTestEntity>();
            this.secondJoiningRepo.Repository.DestroyAll(this.secondJoiningRepo.ContainerName);
        }

        protected abstract RepoInfo GetRepository<TEntity>()
            where TEntity : IPersistableEntity;

        [TestMethod]
        public void WhenAdd_ThenAddsEntity()
        {
            var entity = new TestEntity();
            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);

            this.repo.Repository.Count(this.repo.ContainerName).Should().Be(1);
        }

        [TestMethod]
        public void WhenRemoveAndEntityExists_ThenDeletesEntity()
        {
            var entity = new TestEntity();
            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);

            this.repo.Repository.Remove<TestEntity>(this.repo.ContainerName, entity.Id);

            this.repo.Repository.Count(this.repo.ContainerName).Should().Be(0);
        }

        [TestMethod]
        public void WhenRemoveAndEntityNotExists_ThenReturns()
        {
            this.repo.Repository.Remove<TestEntity>(this.repo.ContainerName, "anid".ToIdentifier());

            this.repo.Repository.Count(this.repo.ContainerName).Should().Be(0);
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var entity =
                this.repo.Repository.Retrieve<TestEntity>(this.repo.ContainerName, "anid".ToIdentifier(),
                    domainFactory);

            entity.Should().BeNull();
        }

        [TestMethod]
        public void WhenGetAndExists_ThenReturnsEntity()
        {
            var entity = new TestEntity
            {
                ABinaryValue = new byte[] {0x01},
                ABooleanValue = true,
                ANullableBooleanValue = true,
                ADoubleValue = 0.1,
                ANullableDoubleValue = 0.1,
                AGuidValue = new Guid("12345678-1111-2222-3333-123456789012"),
                ANullableGuidValue = new Guid("12345678-1111-2222-3333-123456789012"),
                AIntValue = 1,
                ANullableIntValue = 1,
                ALongValue = 2,
                ANullableLongValue = 2,
                AStringValue = "astringvalue",
                ADateTimeUtcValue = DateTime.Today.ToUniversalTime(),
                ANullableDateTimeUtcValue = DateTime.Today.ToUniversalTime(),
                ADateTimeOffsetValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                ANullableDateTimeOffsetValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                AComplexNonValueObjectValue = new ComplexNonValueObject
                {
                    APropertyValue = "avalue"
                },
                AComplexValueObjectValue = ComplexValueObject.Create("avalue", 25, true)
            };

            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);

            var result =
                this.repo.Repository.Retrieve<TestEntity>(this.repo.ContainerName, entity.Id,
                    domainFactory);

            result.Id.Should().Be(entity.Id);
            result.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastPersistedAtUtc.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Utc);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(true);
            result.ANullableBooleanValue.Should().Be(true);
            result.AGuidValue.Should().Be(new Guid("12345678-1111-2222-3333-123456789012"));
            result.ANullableGuidValue.Should().Be(new Guid("12345678-1111-2222-3333-123456789012"));
            result.AIntValue.Should().Be(1);
            result.ANullableIntValue.Should().Be(1);
            result.ALongValue.Should().Be(2);
            result.ANullableLongValue.Should().Be(2);
            result.ADoubleValue.Should().Be(0.1);
            result.ANullableDoubleValue.Should().Be(0.1);
            result.AStringValue.Should().Be("astringvalue");
            result.ADateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ADateTimeUtcValue.Kind.Should().Be(DateTimeKind.Utc);
            result.ANullableDateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ANullableDateTimeUtcValue.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Utc);
            result.ADateTimeOffsetValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.ANullableDateTimeOffsetValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
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
                ANullableBooleanValue = default,
                ADoubleValue = default,
                ANullableDoubleValue = default,
                AGuidValue = default,
                ANullableGuidValue = default,
                AIntValue = default,
                ANullableIntValue = default,
                ALongValue = default,
                ANullableLongValue = default,
                AStringValue = default,
                ADateTimeUtcValue = default,
                ANullableDateTimeUtcValue = default,
                ADateTimeOffsetValue = default,
                ANullableDateTimeOffsetValue = default,
                AComplexNonValueObjectValue = default,
                AComplexValueObjectValue = default
            };

            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);

            var result =
                this.repo.Repository.Retrieve<TestEntity>(this.repo.ContainerName, entity.Id,
                    domainFactory);

            result.Id.Should().Be(entity.Id);
            result.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastPersistedAtUtc.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Utc);
            result.ABinaryValue.Should().BeNull();
            result.ABooleanValue.Should().Be(default);
            result.ANullableBooleanValue.Should().Be(null);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.ANullableGuidValue.Should().Be(null);
            result.AIntValue.Should().Be(default);
            result.ANullableIntValue.Should().Be(null);
            result.ALongValue.Should().Be(default);
            result.ANullableLongValue.Should().Be(null);
            result.ADoubleValue.Should().Be(default);
            result.ANullableDoubleValue.Should().Be(null);
            result.AStringValue.Should().Be(default);
            result.ADateTimeUtcValue.Should().Be(DateTime.MinValue);
            result.ADateTimeUtcValue.Kind.Should().Be(DateTimeKind.Unspecified);
            result.ANullableDateTimeUtcValue.Should().Be(null);
            result.ADateTimeOffsetValue.Should().Be(default);
            result.ANullableDateTimeOffsetValue.Should().Be(null);
            result.AComplexNonValueObjectValue.Should().Be(default);
            result.AComplexValueObjectValue.Should().Be(default);
        }

        [TestMethod]
        public void WhenReplaceExisting_ThenReturnsUpdated()
        {
            var entity = new TestEntity();
            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);

            entity.AStringValue = "updated";
            var updated = this.repo.Repository.Replace(this.repo.ContainerName, entity.Id, entity,
                domainFactory);

            updated.Id.Should().Be(entity.Id);
            updated.AStringValue.Should().Be("updated");
            updated.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
        }

        [TestMethod]
        public void WhenCountAndEmpty_ThenReturnsZero()
        {
            var count = this.repo.Repository.Count(this.repo.ContainerName);

            count.Should().Be(0);
        }

        [TestMethod]
        public void WhenCountAndNotEmpty_ThenReturnsCount()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity(), domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity(), domainFactory);

            var count = this.repo.Repository.Count(this.repo.ContainerName);

            count.Should().Be(2);
        }

        [TestMethod]
        public void WhenQueryAndQueryIsNull_ThenReturnsEmptyResults()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
            {
                AStringValue = "avalue"
            }, domainFactory);

            var results =
                this.repo.Repository.Query<TestEntity>(this.repo.ContainerName, null, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndEmpty_ThenReturnsEmptyResults()
        {
            var query = Query.Empty<TestEntity>();
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
            {
                AStringValue = "avalue"
            }, domainFactory);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndWhereAll_ThenReturnsAllResults()
        {
            var query = Query.From<TestEntity>()
                .WhereAll();
            var entity = this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
            {
                AStringValue = "avalue"
            }, domainFactory);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity.Id);
        }

        [TestMethod]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndNoMatch_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "anothervalue");
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
            {
                AStringValue = "avalue"
            }, domainFactory);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndMatchOne_ThenReturnsResult()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var entity = new TestEntity
            {
                AStringValue = "avalue"
            };
            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity.Id);
        }

        [TestMethod]
        public void WhenQueryAndMatchMany_ThenReturnsResults()
        {
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
            {
                AStringValue = "avalue"
            }, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
            {
                AStringValue = "avalue"
            }, domainFactory);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue1"},
                domainFactory);
            var entity2 = new TestEntity {AStringValue = "avalue2"};
            this.repo.Repository.Add(this.repo.ContainerName, entity2, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, entity2.Id);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue1"},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue2"}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullStringValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue1"},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = null}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNotNullStringValue_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1"}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = null},
                domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.NotEqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.UtcNow;
            var dateTimeOffset2 = DateTimeOffset.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.EqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.UtcNow;
            var dateTimeOffset2 = DateTimeOffset.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.EqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableMinDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.MinValue;
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.EqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueGreaterThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThan, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThanEqualTo, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueLessThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThan, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThanEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueNotEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.NotEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueGreaterThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.GreaterThan, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.GreaterThanEqualTo, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueLessThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.LessThan, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.LessThanEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueNotEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeUtcValue = dateTime2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.NotEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueGreaterThan_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.GreaterThan, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.GreaterThanEqualTo, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueLessThan_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.LessThan, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.LessThanEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueNotEqual_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.NotEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueGreaterThan_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.GreaterThan, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.GreaterThanEqualTo, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueLessThan_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.LessThan, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.LessThanEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueNotEqual_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.NotEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ABooleanValue = false},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ABooleanValue = true}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableBoolValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableBooleanValue = false}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableBooleanValue = true}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableBooleanValue, ConditionOperator.EqualTo, true);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AIntValue = 1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AIntValue = 1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AIntValue = 2},
                domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AIntValue = 1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AIntValue = 2},
                domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableIntValue = 1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueGreaterThan_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableIntValue = 1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueLessThan_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableIntValue = 2},
                domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.LessThan, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueNotEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableIntValue = 1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableIntValue = 2},
                domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ALongValue = 1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ALongValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableLongValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableLongValue = 1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableLongValue = 2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableLongValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ADoubleValue = 1.0},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ADoubleValue = 2.0}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDoubleValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableDoubleValue = 1.0},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableDoubleValue = 2.0}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableDoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForGuidValue_ThenReturnsResult()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AGuidValue = guid1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AGuidValue = guid2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.AGuidValue, ConditionOperator.EqualTo, guid2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableGuidValue_ThenReturnsResult()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ANullableGuidValue = guid1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ANullableGuidValue = guid2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ANullableGuidValue, ConditionOperator.EqualTo, guid2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForBinaryValue_ThenReturnsResult()
        {
            var binary1 = new byte[] {0x01};
            var binary2 = new byte[] {0x01, 0x02};
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {ABinaryValue = binary1},
                domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {ABinaryValue = binary2}, domainFactory);
            var query = Query.From<TestEntity>().Where(e => e.ABinaryValue, ConditionOperator.EqualTo, binary2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            var complex2 = new ComplexNonValueObject {APropertyValue = "avalue2"};
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexNonValueObjectValue = complex1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexNonValueObjectValue = complex2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.EqualTo, complex2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].AComplexNonValueObjectValue.ToString().Should().Be(complex2.ToString());
        }

        [TestMethod]
        public void WhenQueryForNullComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexNonValueObjectValue = complex1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexNonValueObjectValue = null}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.EqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].AComplexNonValueObjectValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexNonValueObjectValue = complex1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexNonValueObjectValue = null}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.NotEqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].AComplexNonValueObjectValue.ToString().Should().Be(complex1.ToString());
        }

        [TestMethod]
        public void WhenQueryForComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            var complex2 = ComplexValueObject.Create("avalue2", 50, false);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexValueObjectValue = complex1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexValueObjectValue = complex2}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.EqualTo, complex2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].AComplexValueObjectValue.Should().Be(complex2);
        }

        [TestMethod]
        public void WhenQueryForNullComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexValueObjectValue = complex1}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexValueObjectValue = null}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.EqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].AComplexValueObjectValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexValueObjectValue = complex1}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AComplexValueObjectValue = null}, domainFactory);
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.NotEqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].AComplexValueObjectValue.Should().Be(complex1);
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
                ADateTimeOffsetValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                AComplexNonValueObjectValue = new ComplexNonValueObject
                {
                    APropertyValue = "avalue"
                },
                AComplexValueObjectValue = ComplexValueObject.Create("avalue", 25, true)
            };

            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);
            var query = Query.From<TestEntity>().WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            var result = results[0];
            result.Id.Should().Be(entity.Id);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(true);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(1);
            result.ALongValue.Should().Be(2);
            result.ADoubleValue.Should().Be(0.1);
            result.AStringValue.Should().Be("astringvalue");
            result.ADateTimeUtcValue.Should().Be(DateTime.Today.ToUniversalTime());
            result.ADateTimeOffsetValue.Should().Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
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
                ADateTimeOffsetValue = DateTimeOffset.UnixEpoch.ToUniversalTime(),
                AComplexNonValueObjectValue = new ComplexNonValueObject
                {
                    APropertyValue = "avalue"
                },
                AComplexValueObjectValue = ComplexValueObject.Create("avalue", 25, true)
            };

            this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);
            var query = Query.From<TestEntity>().WhereAll()
                .Select(e => e.ABinaryValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            var result = results[0];
            result.Id.Should().Be(entity.Id);
            result.ABinaryValue.SequenceEqual(new byte[] {0x01}).Should().BeTrue();
            result.ABooleanValue.Should().Be(false);
            result.AGuidValue.Should().Be(Guid.Empty);
            result.AIntValue.Should().Be(0);
            result.ALongValue.Should().Be(0);
            result.ADoubleValue.Should().Be(0);
            result.AStringValue.Should().Be(null);
            result.ADateTimeUtcValue.Should().Be(DateTime.MinValue);
            result.ADateTimeOffsetValue.Should().Be(DateTimeOffset.MinValue);
            result.AComplexNonValueObjectValue.Should().Be(null);
            result.AComplexValueObjectValue.Should().Be(null);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue1"},
                domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinOnOtherCollection_ThenReturnsOnlyMatchedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1"}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue2"},
                domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue1"}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue3"}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinAndOtherCollectionNotExists_ThenReturnsAllPrimaryResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1"}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinOnOtherCollection_ThenReturnsAllPrimaryResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1"}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue2"}, domainFactory);
            var entity3 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue3"}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue1"}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue5"}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(3);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
            results[2].Id.Should().Be(entity3.Id);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue1"},
                domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnOtherCollection_ThenReturnsAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1", AIntValue = 7}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue2"},
                domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity
                    {AStringValue = "avalue1", AIntValue = 9}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue3"}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].AIntValue.Should().Be(9);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinAndOtherCollectionNotExists_ThenReturnsUnAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1", AIntValue = 7}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].AIntValue.Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinOnOtherCollection_ThenReturnsPartiallyAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue1", AIntValue = 7}, domainFactory);
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue2", AIntValue = 7}, domainFactory);
            var entity3 = this.repo.Repository.Add(this.repo.ContainerName,
                new TestEntity {AStringValue = "avalue3", AIntValue = 7}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity
                    {AStringValue = "avalue1", AIntValue = 9}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue5"}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(3);
            results[0].Id.Should().Be(entity1.Id);
            results[0].AIntValue.Should().Be(9);
            results[1].Id.Should().Be(entity2.Id);
            results[1].AIntValue.Should().Be(7);
            results[2].Id.Should().Be(entity3.Id);
            results[2].AIntValue.Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnMultipleOtherCollections_ThenReturnsAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName, new TestEntity
                {AStringValue = "avalue1", AIntValue = 7, ALongValue = 7}, domainFactory);
            this.repo.Repository.Add(this.repo.ContainerName, new TestEntity {AStringValue = "avalue2"},
                domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity
                    {AStringValue = "avalue1", AIntValue = 9}, domainFactory);
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                new FirstJoiningTestEntity {AStringValue = "avalue3"}, domainFactory);
            this.secondJoiningRepo.Repository.Add(this.secondJoiningRepo.ContainerName,
                new SecondJoiningTestEntity
                    {AStringValue = "avalue1", AIntValue = 9, ALongValue = 8}, domainFactory);
            this.secondJoiningRepo.Repository.Add(this.secondJoiningRepo.ContainerName,
                new SecondJoiningTestEntity {AStringValue = "avalue3"}, domainFactory);
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .AndJoin<SecondJoiningTestEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestEntity, int>(e => e.AIntValue, je => je.AIntValue)
                .SelectFromJoin<SecondJoiningTestEntity, long>(e => e.ALongValue, je => je.ALongValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].AIntValue.Should().Be(9);
            results[0].ALongValue.Should().Be(8);
        }

        [TestMethod]
        public void WhenQueryAndNoOrderBy_ThenReturnsResultsSortedByDateCreatedAscending()
        {
            var entities = CreateMultipleEntities(100);

            var query = Query.From<TestEntity>()
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            VerifyOrderedResultsInReverse(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndNoTake_ThenReturnsAllResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            VerifyOrderedResults(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndZeroTake_ThenReturnsNoResults()
        {
            CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Take(0);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndTakeLessThanAvailable_ThenReturnsAsManyResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Take(10);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query, domainFactory);

            results.Count.Should().Be(0);
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
                this.repo.Repository.Add(this.repo.ContainerName, entity, domainFactory);
                createdIdentifiers.Add(entity.Id);
                IntroduceTimeDelayForSortingDates();
            }, count);

            return createdIdentifiers;
        }

        private static void VerifyOrderedResultsInReverse(List<TestEntity> results, List<Identifier> entities,
            int? offset = null, int? limit = null)
        {
            entities.Reverse();
            VerifyOrderedResults(results, entities, offset, limit);
        }

        private static void VerifyOrderedResults(List<TestEntity> results, IReadOnlyList<Identifier> entities,
            int? offset = null, int? limit = null)
        {
            var expectedResultCount = limit ?? entities.Count;
            results.Count.Should().Be(expectedResultCount);

            var resultIndex = 0;
            var entityCount = 0;
            results.ForEach(result =>
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