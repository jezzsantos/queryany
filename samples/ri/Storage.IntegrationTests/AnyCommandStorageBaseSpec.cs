using System;
using System.Linq;
using Api.Common;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Funq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny;
using ServiceStack;
using Storage.Interfaces;

namespace Storage.IntegrationTests
{
    public abstract class AnyCommandStorageBaseSpec

    {
        protected static readonly ILogger Logger = new Logger<AnyCommandStorageBaseSpec>(new NullLoggerFactory());
        private ICommandStorage<TestEntity> commandStorage;
        private Container container;
        private IDomainFactory domainFactory;

        [TestInitialize]
        public void Initialize()
        {
            this.container = new Container();
            this.container.AddSingleton(Logger);
            this.domainFactory = new DomainFactory(new FuncDependencyContainer(this.container));
            this.domainFactory.RegisterTypesFromAssemblies(typeof(AnyCommandStorageBaseSpec).Assembly);
            this.commandStorage =
                GetCommandStore<TestEntity>(typeof(TestEntity).GetEntityNameSafe(), this.domainFactory);
            this.commandStorage.DestroyAll();
        }

        protected abstract ICommandStorage<TEntity> GetCommandStore<TEntity>(string containerName,
            IDomainFactory domainFactory)
            where TEntity : IPersistableEntity;

        [TestMethod]
        public void WhenDeleteAndEntityExists_ThenDeletesEntity()
        {
            var entity = new TestEntity();
            this.commandStorage.Upsert(entity);

            this.commandStorage.Delete(entity.Id);

            this.commandStorage.Count().Should().Be(0);
        }

        [TestMethod]
        public void WhenDeleteAndEntityNotExists_ThenReturns()
        {
            this.commandStorage.Delete("anid".ToIdentifier());

            this.commandStorage.Count().Should().Be(0);
        }

        [TestMethod]
        public void WhenDeleteAndIdIsEmpty_ThenThrows()
        {
            this.commandStorage.Invoking(x => x.Delete(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenGetAndNotExists_ThenReturnsNull()
        {
            var entity = this.commandStorage.Get("anid".ToIdentifier());

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

            this.commandStorage.Upsert(entity);

            var result = this.commandStorage.Get(entity.Id);

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

            this.commandStorage.Upsert(entity);

            var result = this.commandStorage.Get(entity.Id);

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
        public void WhenGetAndIdIsEmpty_ThenThrows()
        {
            this.commandStorage.Invoking(x => x.Get(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenUpsertAndExists_ThenReturnsUpdated()
        {
            var entity = new TestEntity();
            this.commandStorage.Upsert(entity);

            entity.AStringValue = "updated";
            var updated = this.commandStorage.Upsert(entity);

            updated.Id.Should().Be(entity.Id);
            updated.AStringValue.Should().Be("updated");
            updated.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
        }

        [TestMethod]
        public void WhenUpsertAndNotExists_ThenAdds()
        {
            var entity = new TestEntity
            {
                AStringValue = "updated"
            };

            var added = this.commandStorage.Upsert(entity);
            this.commandStorage.Count().Should().Be(1);

            added.Id.Should().Be(entity.Id);
            added.LastPersistedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(0.5));
        }

        [TestMethod]
        public void WhenUpsertAndEmptyId_ThenThrows()
        {
            var entity = new TestEntity(Identifier.Empty());

            this.commandStorage.Invoking(x => x.Upsert(entity))
                .Should().Throw<ResourceNotFoundException>();
        }

        [TestMethod]
        public void WhenCountAndEmpty_ThenReturnsZero()
        {
            var count = this.commandStorage.Count();

            count.Should().Be(0);
        }

        [TestMethod]
        public void WhenCountAndNotEmpty_ThenReturnsCount()
        {
            this.commandStorage.Upsert(new TestEntity());
            this.commandStorage.Upsert(new TestEntity());

            var count = this.commandStorage.Count();

            count.Should().Be(2);
        }
    }
}