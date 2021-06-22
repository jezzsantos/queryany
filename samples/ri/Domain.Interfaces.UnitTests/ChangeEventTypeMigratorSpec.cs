using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Properties;
using FluentAssertions;
using UnitTesting.Common;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class ChangeEventTypeMigratorSpec
    {
        private readonly Dictionary<string, string> mappings;
        private readonly ChangeEventTypeMigrator migrator;

        public ChangeEventTypeMigratorSpec()
        {
            this.mappings = new Dictionary<string, string>();
            this.migrator = new ChangeEventTypeMigrator(this.mappings);
        }

        [Fact]
        public void WhenRehydrateAndTypeKnown_ThenReturnsNewInstance()
        {
            var eventJson = EntityEvent.ToData(new TestChangeEvent {EntityId = "anentityid"});
            var result = this.migrator.Rehydrate("aneventid", eventJson, typeof(TestChangeEvent).AssemblyQualifiedName);

            result.Should().BeOfType<TestChangeEvent>();
            result.As<TestChangeEvent>().EntityId.Should().Be("anentityid");
        }

        [Fact]
        public void WhenRehydrateAndUnknownType_ThenThrows()
        {
            var eventJson = EntityEvent.ToData(new TestChangeEvent {EntityId = "anentityid"});
            this.migrator
                .Invoking(x => x.Rehydrate("aneventid", eventJson, "anunknowntype"))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ChangeEventMigrator_UnknownType);
        }

        [Fact]
        public void WhenRehydrateAndUnknownTypeAndMappingStillNotExist_ThenThrows()
        {
            this.mappings.Add("anunknowntype", "anotherunknowntype");
            var eventJson = EntityEvent.ToData(new TestChangeEvent {EntityId = "anentityid"});
            this.migrator
                .Invoking(x => x.Rehydrate("aneventid", eventJson, "anunknowntype"))
                .Should().Throw<InvalidOperationException>()
                .WithMessageLike(Resources.ChangeEventMigrator_UnknownType);
        }

        [Fact]
        public void WhenRehydrateAndUnknownTypeAndMappingExists_ThenReturnsNewInstance()
        {
            this.mappings.Add("anunknowntype", typeof(TestRenamedChangeEvent).AssemblyQualifiedName);
            var eventJson = EntityEvent.ToData(new TestChangeEvent {EntityId = "anentityid"});
            var result = this.migrator.Rehydrate("aneventid", eventJson, "anunknowntype");

            result.Should().BeOfType<TestRenamedChangeEvent>();
            result.As<TestRenamedChangeEvent>().EntityId.Should().Be("anentityid");
        }
    }

    public class TestChangeEvent : IChangeEvent
    {
        public string EntityId { get; set; }

        public DateTime ModifiedUtc { get; set; }
    }

    public class TestRenamedChangeEvent : IChangeEvent
    {
        public string EntityId { get; set; }

        public DateTime ModifiedUtc { get; set; }
    }
}