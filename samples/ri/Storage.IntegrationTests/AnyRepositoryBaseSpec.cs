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
        private IDomainFactory domainFactory;
        private RepoInfo firstJoiningRepo;
        private RepoInfo repo;
        private RepoInfo secondJoiningRepo;

        protected static void InitializeAllTests()
        {
            container = new Container();
            container.AddSingleton(Logger);
        }

        [TestInitialize]
        public void Initialize()
        {
            this.domainFactory = DomainFactory.CreateRegistered(new FuncDependencyContainer(container),
                typeof(TestEntity).Assembly);
            this.repo = GetRepository<TestEntity>();
            this.repo.Repository.DestroyAll(this.repo.ContainerName);
            this.firstJoiningRepo = GetRepository<FirstJoiningTestQueryableEntity>();
            this.firstJoiningRepo.Repository.DestroyAll(this.firstJoiningRepo.ContainerName);
            this.secondJoiningRepo = GetRepository<SecondJoiningTestQueryableEntity>();
            this.secondJoiningRepo.Repository.DestroyAll(this.secondJoiningRepo.ContainerName);
        }

        protected abstract RepoInfo GetRepository<TQueryableEntity>()
            where TQueryableEntity : IQueryableEntity;

        [TestMethod]
        public void WhenAddWithNullEntity_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Add(this.repo.ContainerName, null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenAddWithNullContainer_ThenThrows()
        {
            var entity = new CommandEntity("anid");
            this.repo.Repository
                .Invoking(x => x.Add(null, entity))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenAdd_ThenAddsEntity()
        {
            var entity = new CommandEntity("anid");
            this.repo.Repository.Add(this.repo.ContainerName, entity);

            this.repo.Repository.Count(this.repo.ContainerName).Should().Be(1);
        }

        [TestMethod]
        public void WhenRemoveWithNullId_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Remove(this.repo.ContainerName, null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenRemoveWithNullContainer_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Remove(null, "anid".ToIdentifier()))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenRemoveAndEntityExists_ThenDeletesEntity()
        {
            var entity = new CommandEntity("anid");
            this.repo.Repository.Add(this.repo.ContainerName, entity);

            this.repo.Repository.Remove(this.repo.ContainerName, entity.Id);

            this.repo.Repository.Count(this.repo.ContainerName).Should().Be(0);
        }

        [TestMethod]
        public void WhenRemoveAndEntityNotExists_ThenReturns()
        {
            this.repo.Repository.Remove(this.repo.ContainerName, "anid".ToIdentifier());

            this.repo.Repository.Count(this.repo.ContainerName).Should().Be(0);
        }

        [TestMethod]
        public void WhenRetrieveWithNullId_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Retrieve(this.repo.ContainerName, null, RepositoryEntityMetadata.Empty))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenRetrieveWithNullContainer_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Retrieve(null, "anid".ToIdentifier(), RepositoryEntityMetadata.Empty))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenRetrieveWithNullMetadata_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Retrieve(this.repo.ContainerName, "anid".ToIdentifier(), null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenRetrieveAndNotExists_ThenReturnsNull()
        {
            var entity =
                this.repo.Repository.Retrieve(this.repo.ContainerName, "anid".ToIdentifier(),
                    RepositoryEntityMetadata.FromType<TestEntity>());

            entity.Should().BeNull();
        }

        [TestMethod]
        public void WhenRetrieveAndExists_ThenReturnsEntity()
        {
            var entity = CommandEntity.FromType(new TestEntity
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
            });

            this.repo.Repository.Add(this.repo.ContainerName, entity);

            var result =
                this.repo.Repository.Retrieve(this.repo.ContainerName, entity.Id,
                    RepositoryEntityMetadata.FromType<TestEntity>());

            result.Id.Should().Be(entity.Id);
            result.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastPersistedAtUtc.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Utc);
            result.GetValueOrDefault<byte[]>(nameof(TestEntity.ABinaryValue)).SequenceEqual(new byte[] {0x01})
                .Should().BeTrue();
            result.GetValueOrDefault<bool>(nameof(TestEntity.ABooleanValue)).Should().Be(true);
            result.GetValueOrDefault<bool?>(nameof(TestEntity.ANullableBooleanValue)).Should().Be(true);
            result.GetValueOrDefault<Guid>(nameof(TestEntity.AGuidValue)).Should()
                .Be(new Guid("12345678-1111-2222-3333-123456789012"));
            result.GetValueOrDefault<Guid?>(nameof(TestEntity.ANullableGuidValue)).Should()
                .Be(new Guid("12345678-1111-2222-3333-123456789012"));
            result.GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(1);
            result.GetValueOrDefault<int?>(nameof(TestEntity.ANullableIntValue)).Should().Be(1);
            result.GetValueOrDefault<long>(nameof(TestEntity.ALongValue)).Should().Be(2);
            result.GetValueOrDefault<long?>(nameof(TestEntity.ANullableLongValue)).Should().Be(2);
            result.GetValueOrDefault<double>(nameof(TestEntity.ADoubleValue)).Should().Be(0.1);
            result.GetValueOrDefault<double?>(nameof(TestEntity.ANullableDoubleValue)).Should().Be(0.1);
            result.GetValueOrDefault<string>(nameof(TestEntity.AStringValue)).Should().Be("astringvalue");
            result.GetValueOrDefault<DateTime>(nameof(TestEntity.ADateTimeUtcValue)).Should()
                .Be(DateTime.Today.ToUniversalTime());
            result.GetValueOrDefault<DateTime>(nameof(TestEntity.ADateTimeUtcValue)).Kind.Should()
                .Be(DateTimeKind.Utc);
            result.GetValueOrDefault<DateTime?>(nameof(TestEntity.ANullableDateTimeUtcValue)).Should()
                .Be(DateTime.Today.ToUniversalTime());
            result.GetValueOrDefault<DateTime?>(nameof(TestEntity.ANullableDateTimeUtcValue))
                .GetValueOrDefault()
                .Kind.Should().Be(DateTimeKind.Utc);
            result.GetValueOrDefault<DateTimeOffset>(nameof(TestEntity.ADateTimeOffsetValue)).Should()
                .Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.GetValueOrDefault<DateTimeOffset?>(nameof(TestEntity.ANullableDateTimeOffsetValue)).Should()
                .Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .ToString().Should().Be(new ComplexNonValueObject {APropertyValue = "avalue"}.ToString());
            result.GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                    this.domainFactory).Should()
                .Be(ComplexValueObject.Create("avalue", 25, true));
        }

        [TestMethod]
        public void WhenRetrieveAndExistsWithDefaultValues_ThenReturnsEntity()
        {
            var entity = CommandEntity.FromType(new TestEntity
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
            });

            this.repo.Repository.Add(this.repo.ContainerName, entity);

            var result =
                this.repo.Repository.Retrieve(this.repo.ContainerName, entity.Id,
                    RepositoryEntityMetadata.FromType<TestEntity>());

            result.Id.Should().Be(entity.Id);
            result.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
            result.LastPersistedAtUtc.GetValueOrDefault().Kind.Should().Be(DateTimeKind.Utc);
            result.GetValueOrDefault<byte[]>(nameof(TestEntity.ABinaryValue)).Should().BeNull();
            result.GetValueOrDefault<bool>(nameof(TestEntity.ABooleanValue)).Should().Be(default);
            result.GetValueOrDefault<bool?>(nameof(TestEntity.ANullableBooleanValue)).Should().Be(null);
            result.GetValueOrDefault<Guid>(nameof(TestEntity.AGuidValue)).Should().Be(Guid.Empty);
            result.GetValueOrDefault<Guid?>(nameof(TestEntity.ANullableGuidValue)).Should().Be(null);
            result.GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(default);
            result.GetValueOrDefault<int?>(nameof(TestEntity.ANullableIntValue)).Should().Be(null);
            result.GetValueOrDefault<long>(nameof(TestEntity.ALongValue)).Should().Be(default);
            result.GetValueOrDefault<long?>(nameof(TestEntity.ANullableLongValue)).Should().Be(null);
            result.GetValueOrDefault<double>(nameof(TestEntity.ADoubleValue)).Should().Be(default);
            result.GetValueOrDefault<double?>(nameof(TestEntity.ANullableDoubleValue)).Should().Be(null);
            result.GetValueOrDefault<string>(nameof(TestEntity.AStringValue)).Should().Be(default);
            result.GetValueOrDefault<DateTime>(nameof(TestEntity.ADateTimeUtcValue)).Should()
                .Be(DateTime.MinValue);
            result.GetValueOrDefault<DateTime>(nameof(TestEntity.ADateTimeUtcValue)).Kind.Should()
                .Be(DateTimeKind.Unspecified);
            result.GetValueOrDefault<DateTime?>(nameof(TestEntity.ANullableDateTimeUtcValue)).Should()
                .Be(null);
            result.GetValueOrDefault<DateTimeOffset>(nameof(TestEntity.ADateTimeOffsetValue)).Should()
                .Be(default);
            result.GetValueOrDefault<DateTimeOffset?>(nameof(TestEntity.ANullableDateTimeOffsetValue)).Should()
                .Be(null);
            result.GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .Should()
                .Be(default);
            result.GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                    this.domainFactory).Should()
                .Be(default);
        }

        [TestMethod]
        public void WhenReplaceWithNullId_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Replace(this.repo.ContainerName, null, new CommandEntity("anid")))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenReplaceWithNullContainer_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Replace(null, "anid".ToIdentifier(), new CommandEntity("anid")))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenReplaceWithNullEntity_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Replace(this.repo.ContainerName, "anid".ToIdentifier(), null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenReplaceExisting_ThenReturnsUpdated()
        {
            var entity = new CommandEntity("anid");
            this.repo.Repository.Add(this.repo.ContainerName, entity);

            entity.Add(nameof(TestEntity.AStringValue), "updated", typeof(string));
            var updated = this.repo.Repository.Replace(this.repo.ContainerName, entity.Id, entity);

            updated.Id.Should().Be(entity.Id);
            updated.Properties.GetValueOrDefault<string>(nameof(TestEntity.AStringValue)).Should()
                .Be("updated");
            updated.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
        }

        [TestMethod]
        public void WhenCountWithNullContainer_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.Count(null))
                .Should().Throw<ArgumentNullException>();
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
            this.repo.Repository.Add(this.repo.ContainerName, new CommandEntity("anid1"));
            this.repo.Repository.Add(this.repo.ContainerName, new CommandEntity("anid2"));

            var count = this.repo.Repository.Count(this.repo.ContainerName);

            count.Should().Be(2);
        }

        [TestMethod]
        public void WhenDestroyAllWithNullContainer_ThenThrows()
        {
            this.repo.Repository
                .Invoking(x => x.DestroyAll(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenQueryAndQueryIsNull_ThenReturnsEmptyResults()
        {
            this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            }));

            var results =
                this.repo.Repository.Query<TestEntity>(this.repo.ContainerName, null,
                    RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndEmpty_ThenReturnsEmptyResults()
        {
            var query = Query.Empty<TestEntity>();
            this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            }));

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndWhereAll_ThenReturnsAllResults()
        {
            var query = Query.From<TestEntity>()
                .WhereAll();
            var entity = this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            }));

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity.Id);
        }

        [TestMethod]
        public void WhenQueryAndNoEntities_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>()
                .Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndNoMatch_ThenReturnsEmptyResults()
        {
            var query = Query.From<TestEntity>()
                .Where(e => e.AStringValue, ConditionOperator.EqualTo, "anothervalue");
            this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            }));

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryAndMatchOne_ThenReturnsResult()
        {
            var query = Query.From<TestEntity>()
                .Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var entity = CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            });
            this.repo.Repository.Add(this.repo.ContainerName, entity);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity.Id);
        }

        [TestMethod]
        public void WhenQueryAndMatchMany_ThenReturnsResults()
        {
            var query = Query.From<TestEntity>()
                .Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue");
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            }));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
            {
                AStringValue = "avalue"
            }));

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryWithId_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var entity2 = CommandEntity.FromType(new TestEntity {AStringValue = "avalue2"});
            this.repo.Repository.Add(this.repo.ContainerName, entity2);
            var query = Query.From<TestEntity>().Where(e => e.Id, ConditionOperator.EqualTo, entity2.Id);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForStringValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue2"}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AStringValue, ConditionOperator.EqualTo, "avalue2");

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullStringValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = null}));
            var query = Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.EqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNotNullStringValue_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = null}));
            var query =
                Query.From<TestEntity>().Where(e => e.AStringValue, ConditionOperator.NotEqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.UtcNow;
            var dateTimeOffset2 = DateTimeOffset.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.EqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.UtcNow;
            var dateTimeOffset2 = DateTimeOffset.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.EqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableMinDateTimeValue_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.MinValue;
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.EqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableMinDateTimeOffsetValue_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.MinValue;
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.EqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueGreaterThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThan, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.GreaterThanEqualTo, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThan, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeUtcValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.LessThanEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeUtcValue, ConditionOperator.NotEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueGreaterThan_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.GreaterThan, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.GreaterThanEqualTo, dateTime1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.LessThan, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeUtcValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTime1 = DateTime.UtcNow;
            var dateTime2 = DateTime.UtcNow.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.LessThanEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeUtcValue = dateTime2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeUtcValue, ConditionOperator.NotEqualTo, dateTime2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueGreaterThan_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.GreaterThan, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.GreaterThanEqualTo, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.LessThan, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForDateTimeOffsetValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.LessThanEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ADateTimeOffsetValue, ConditionOperator.NotEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueGreaterThan_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.GreaterThan, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.GreaterThanEqualTo, dateTimeOffset1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.LessThan, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDateTimeOffsetValueLessThanOrEqual_ThenReturnsResult()
        {
            var dateTimeOffset1 = DateTimeOffset.Now;
            var dateTimeOffset2 = DateTimeOffset.Now.AddDays(1);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.LessThanEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDateTimeOffsetValue = dateTimeOffset2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDateTimeOffsetValue, ConditionOperator.NotEqualTo, dateTimeOffset2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForBoolValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ABooleanValue = false}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ABooleanValue = true}));
            var query = Query.From<TestEntity>().Where(e => e.ABooleanValue, ConditionOperator.EqualTo, true);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableBoolValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableBooleanValue = false}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableBooleanValue = true}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableBooleanValue, ConditionOperator.EqualTo, true);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThan_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThan_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThan, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForIntValueNotEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AIntValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.AIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueGreaterThan_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableIntValue, ConditionOperator.GreaterThan, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueGreaterThanOrEqualTo_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableIntValue, ConditionOperator.GreaterThanEqualTo, 1);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueLessThan_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 2}));
            var query =
                Query.From<TestEntity>().Where(e => e.ANullableIntValue, ConditionOperator.LessThan, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueLessThanOrEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableIntValue, ConditionOperator.LessThanEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(2);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableIntValueNotEqual_ThenReturnsResult()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableIntValue = 2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableIntValue, ConditionOperator.NotEqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryForLongValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ALongValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ALongValue = 2}));
            var query = Query.From<TestEntity>().Where(e => e.ALongValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableLongValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableLongValue = 1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableLongValue = 2}));
            var query =
                Query.From<TestEntity>().Where(e => e.ANullableLongValue, ConditionOperator.EqualTo, 2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForDoubleValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADoubleValue = 1.0}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ADoubleValue = 2.0}));
            var query = Query.From<TestEntity>().Where(e => e.ADoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableDoubleValue_ThenReturnsResult()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDoubleValue = 1.0}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableDoubleValue = 2.0}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableDoubleValue, ConditionOperator.EqualTo, 2.0);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForGuidValue_ThenReturnsResult()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AGuidValue = guid1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AGuidValue = guid2}));
            var query = Query.From<TestEntity>().Where(e => e.AGuidValue, ConditionOperator.EqualTo, guid2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForNullableGuidValue_ThenReturnsResult()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableGuidValue = guid1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ANullableGuidValue = guid2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.ANullableGuidValue, ConditionOperator.EqualTo, guid2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForBinaryValue_ThenReturnsResult()
        {
            var binary1 = new byte[] {0x01};
            var binary2 = new byte[] {0x01, 0x02};
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ABinaryValue = binary1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {ABinaryValue = binary2}));
            var query =
                Query.From<TestEntity>().Where(e => e.ABinaryValue, ConditionOperator.EqualTo, binary2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
        }

        [TestMethod]
        public void WhenQueryForComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            var complex2 = new ComplexNonValueObject {APropertyValue = "avalue2"};
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexNonValueObjectValue = complex1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexNonValueObjectValue = complex2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.EqualTo, complex2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .ToString().Should().Be(complex2.ToString());
        }

        [TestMethod]
        public void WhenQueryForNullComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexNonValueObjectValue = complex1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexNonValueObjectValue = null}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.EqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .Should().BeNull();
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexNonValueObjectValue_ThenReturnsResult()
        {
            var complex1 = new ComplexNonValueObject {APropertyValue = "avalue1"};
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexNonValueObjectValue = complex1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexNonValueObjectValue = null}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexNonValueObjectValue, ConditionOperator.NotEqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .ToString().Should().Be(complex1.ToString());
        }

        [TestMethod]
        public void WhenQueryForComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            var complex2 = ComplexValueObject.Create("avalue2", 50, false);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexValueObjectValue = complex1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexValueObjectValue = complex2}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.EqualTo, complex2);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                this.domainFactory).Should().BeEquivalentTo(complex2);
        }

        [TestMethod]
        public void WhenQueryForNullComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexValueObjectValue = complex1}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexValueObjectValue = null}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.EqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity2.Id);
            results[0].GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                this.domainFactory).Should().BeNull();
        }

        [TestMethod]
        public void WhenQueryForNotEqualNullComplexValueObjectValue_ThenReturnsResult()
        {
            var complex1 = ComplexValueObject.Create("avalue1", 25, true);
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexValueObjectValue = complex1}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AComplexValueObjectValue = null}));
            var query = Query.From<TestEntity>()
                .Where(e => e.AComplexValueObjectValue, ConditionOperator.NotEqualTo, null);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                this.domainFactory).Should().BeEquivalentTo(complex1);
        }

        [TestMethod]
        public void WhenQueryAndNoSelects_ThenReturnsResultWithAllPropertiesPopulated()
        {
            var entity = CommandEntity.FromType(new TestEntity
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
            });

            this.repo.Repository.Add(this.repo.ContainerName, entity);
            var query = Query.From<TestEntity>().WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            var result = results[0];
            result.Id.Should().Be(entity.Id);
            result.GetValueOrDefault<byte[]>(nameof(TestEntity.ABinaryValue)).SequenceEqual(new byte[] {0x01})
                .Should().BeTrue();
            result.GetValueOrDefault<bool>(nameof(TestEntity.ABooleanValue)).Should().Be(true);
            result.GetValueOrDefault<Guid>(nameof(TestEntity.AGuidValue)).Should().Be(Guid.Empty);
            result.GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(1);
            result.GetValueOrDefault<long>(nameof(TestEntity.ALongValue)).Should().Be(2);
            result.GetValueOrDefault<double>(nameof(TestEntity.ADoubleValue)).Should().Be(0.1);
            result.GetValueOrDefault<string>(nameof(TestEntity.AStringValue)).Should().Be("astringvalue");
            result.GetValueOrDefault<DateTime>(nameof(TestEntity.ADateTimeUtcValue)).Should()
                .Be(DateTime.Today.ToUniversalTime());
            result.GetValueOrDefault<DateTimeOffset>(nameof(TestEntity.ADateTimeOffsetValue)).Should()
                .Be(DateTimeOffset.UnixEpoch.ToUniversalTime());
            result.GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .ToString().Should().Be(new ComplexNonValueObject {APropertyValue = "avalue"}.ToString());
            result.GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                this.domainFactory).Should().BeEquivalentTo(ComplexValueObject.Create("avalue", 25, true));
        }

        [TestMethod]
        public void WhenQueryAndSelect_ThenReturnsResultWithOnlySelectedPropertiesPopulated()
        {
            var entity = CommandEntity.FromType(new TestEntity
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
            });

            this.repo.Repository.Add(this.repo.ContainerName, entity);
            var query = Query.From<TestEntity>().WhereAll()
                .Select(e => e.ABinaryValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            var result = results[0];
            result.Id.Should().Be(entity.Id);
            result.GetValueOrDefault<byte[]>(nameof(TestEntity.ABinaryValue)).SequenceEqual(new byte[] {0x01})
                .Should().BeTrue();
            result.GetValueOrDefault<bool>(nameof(TestEntity.ABooleanValue)).Should().Be(false);
            result.GetValueOrDefault<Guid>(nameof(TestEntity.AGuidValue)).Should().Be(Guid.Empty);
            result.GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(0);
            result.GetValueOrDefault<long>(nameof(TestEntity.ALongValue)).Should().Be(0);
            result.GetValueOrDefault<double>(nameof(TestEntity.ADoubleValue)).Should().Be(0);
            result.GetValueOrDefault<string>(nameof(TestEntity.AStringValue)).Should().Be(null);
            result.GetValueOrDefault<DateTime>(nameof(TestEntity.ADateTimeUtcValue)).Should()
                .Be(DateTime.MinValue);
            result.GetValueOrDefault<DateTimeOffset>(nameof(TestEntity.ADateTimeOffsetValue)).Should()
                .Be(DateTimeOffset.MinValue);
            result.GetValueOrDefault<ComplexNonValueObject>(nameof(TestEntity.AComplexNonValueObjectValue))
                .Should().BeNull();
            result.GetValueOrDefault<ComplexValueObject>(nameof(TestEntity.AComplexValueObjectValue),
                this.domainFactory).Should().BeNull();
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithInnerJoinOnOtherCollection_ThenReturnsOnlyMatchedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue2"}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue1"}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue3"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinAndOtherCollectionNotExists_ThenReturnsAllPrimaryResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
        }

        [TestMethod]
        public void WhenQueryWithLeftJoinOnOtherCollection_ThenReturnsAllPrimaryResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue2"}));
            var entity3 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue3"}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue1"}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue5"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(3);
            results[0].Id.Should().Be(entity1.Id);
            results[1].Id.Should().Be(entity2.Id);
            results[2].Id.Should().Be(entity3.Id);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinAndOtherCollectionNotExists_ThenReturnsNoResults()
        {
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestQueryableEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnOtherCollection_ThenReturnsAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1", AIntValue = 7}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue2"}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity
                    {AStringValue = "avalue1", AIntValue = 9}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue3"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestQueryableEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(9);
            results[0].GetValueOrDefault<bool>(nameof(TestEntity.ABooleanValue)).Should().Be(false);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinAndOtherCollectionNotExists_ThenReturnsUnAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1", AIntValue = 7}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestQueryableEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromLeftJoinOnOtherCollection_ThenReturnsPartiallyAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue1", AIntValue = 7}));
            var entity2 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue2", AIntValue = 7}));
            var entity3 = this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue3", AIntValue = 7}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity
                    {AStringValue = "avalue1", AIntValue = 9}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue5"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue, JoinType.Left)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestQueryableEntity, int>(e => e.AIntValue, je => je.AIntValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(3);
            results[0].Id.Should().Be(entity1.Id);
            results[0].GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(9);
            results[1].Id.Should().Be(entity2.Id);
            results[1].GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(7);
            results[2].Id.Should().Be(entity3.Id);
            results[2].GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(7);
        }

        [TestMethod]
        public void WhenQueryWithSelectFromInnerJoinOnMultipleOtherCollections_ThenReturnsAggregatedResults()
        {
            var entity1 = this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(new TestEntity
                {AStringValue = "avalue1", AIntValue = 7, ALongValue = 7}));
            this.repo.Repository.Add(this.repo.ContainerName,
                CommandEntity.FromType(new TestEntity {AStringValue = "avalue2"}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity
                    {AStringValue = "avalue1", AIntValue = 9}));
            this.firstJoiningRepo.Repository.Add(this.firstJoiningRepo.ContainerName,
                CommandEntity.FromType(new FirstJoiningTestQueryableEntity {AStringValue = "avalue3"}));
            this.secondJoiningRepo.Repository.Add(this.secondJoiningRepo.ContainerName,
                CommandEntity.FromType(new SecondJoiningTestQueryableEntity
                    {AStringValue = "avalue1", AIntValue = 9, ALongValue = 8}));
            this.secondJoiningRepo.Repository.Add(this.secondJoiningRepo.ContainerName,
                CommandEntity.FromType(new SecondJoiningTestQueryableEntity {AStringValue = "avalue3"}));
            var query = Query.From<TestEntity>()
                .Join<FirstJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .AndJoin<SecondJoiningTestQueryableEntity, string>(e => e.AStringValue, j => j.AStringValue)
                .WhereAll()
                .SelectFromJoin<FirstJoiningTestQueryableEntity, int>(e => e.AIntValue, je => je.AIntValue)
                .SelectFromJoin<SecondJoiningTestQueryableEntity, long>(e => e.ALongValue, je => je.ALongValue);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(1);
            results[0].Id.Should().Be(entity1.Id);
            results[0].GetValueOrDefault<int>(nameof(TestEntity.AIntValue)).Should().Be(9);
            results[0].GetValueOrDefault<long>(nameof(TestEntity.ALongValue)).Should().Be(8);
        }

        [TestMethod]
        public void WhenQueryAndNoOrderBy_ThenReturnsResultsSortedByLastPersistedAtUtcAscending()
        {
            var entities = CreateMultipleEntities(100);

            var query = Query.From<TestEntity>()
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            VerifyOrderedResultsInReverse(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndNoTake_ThenReturnsAllResults()
        {
            var entities =
                CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll();

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            VerifyOrderedResults(results, entities);
        }

        [TestMethod]
        public void WhenQueryAndZeroTake_ThenReturnsNoResults()
        {
            CreateMultipleEntities(100, (counter, entity) => entity.AStringValue = $"avalue{counter:000}");

            var query = Query.From<TestEntity>()
                .WhereAll()
                .Take(0);

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

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

            var results = this.repo.Repository.Query(this.repo.ContainerName, query,
                RepositoryEntityMetadata.FromType<TestEntity>());

            results.Count.Should().Be(0);
        }

        private List<Identifier> CreateMultipleEntities(int count, Action<int, TestEntity> factory = null)
        {
            static void WaitSomeTimeToIntroduceTimeDelayForSortingDates()
            {
                Thread.Sleep(20);
            }

            var createdIdentifiers = new List<Identifier>();
            Repeat.Times(counter =>
            {
                var entity = new TestEntity();
                factory?.Invoke(counter, entity);
                this.repo.Repository.Add(this.repo.ContainerName, CommandEntity.FromType(entity));
                createdIdentifiers.Add(entity.Id);
                WaitSomeTimeToIntroduceTimeDelayForSortingDates();
            }, count);

            return createdIdentifiers;
        }

        private static void VerifyOrderedResultsInReverse(List<QueryEntity> results, List<Identifier> entities,
            int? offset = null, int? limit = null)
        {
            entities.Reverse();
            VerifyOrderedResults(results, entities, offset, limit);
        }

        private static void VerifyOrderedResults(List<QueryEntity> results, IReadOnlyList<Identifier> entities,
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