using System;
using System.Collections.Generic;
using System.Linq;
using Api.Common.Properties;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using UnitTesting.Common;
using Xunit;

namespace Api.Common.UnitTests
{
    [Trait("Category", "Unit")]
    public class DomainFactorySpec
    {
        private readonly DomainFactory factory;

        public DomainFactorySpec()
        {
            var recorder = new Mock<IRecorder>();
            var identifierFactory = new Mock<IIdentifierFactory>();
            identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier);
            var dependencyContainer = new Mock<IDependencyContainer>();
            dependencyContainer.Setup(dc => dc.Resolve<IRecorder>())
                .Returns(recorder.Object);
            dependencyContainer.Setup(dc => dc.Resolve<IIdentifierFactory>())
                .Returns(identifierFactory.Object);

            this.factory = new DomainFactory(dependencyContainer.Object);
        }

        [Fact]
        public void WhenRegisterAndNoAssemblies_ThenRegistersNone()
        {
            this.factory.RegisterDomainTypesFromAssemblies();

            this.factory.EntityFactories.Count.Should().Be(0);
            this.factory.ValueObjectFactories.Count.Should().Be(0);
        }

        [Fact]
        public void WhenRegisterAndAssemblyContainsNoFactories_ThenRegistersNone()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(Exactly).Assembly);

            this.factory.EntityFactories.Count.Should().Be(0);
            this.factory.ValueObjectFactories.Count.Should().Be(0);
        }

        [Fact]
        public void WhenRegisterAndAssemblyContainsFactories_ThenRegistersFactories()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(DomainFactorySpec).Assembly);

            this.factory.EntityFactories.Count.Should().Be(1);
            this.factory.EntityFactories.First().Key.Should().Be(typeof(TestEntity));
            this.factory.ValueObjectFactories.Count.Should().Be(1);
            this.factory.ValueObjectFactories.First().Key.Should().Be(typeof(TestValueObject));
        }

        [Fact]
        public void WhenCreateEntityAndTypeNotExist_ThenThrows()
        {
            this.factory
                .Invoking(x => x
                    .RehydrateEntity(typeof(TestEntity), new Dictionary<string, object>())).Should()
                .Throw<InvalidOperationException>()
                .WithMessageLike(Resources.DomainFactory_EntityTypeNotFound);
        }

        [Fact]
        public void WhenCreateEntityAndExists_ThenReturnsEntityInstance()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(DomainFactorySpec).Assembly);

            var result = (TestEntity) this.factory.RehydrateEntity(typeof(TestEntity), new Dictionary<string, object>
            {
                {nameof(IIdentifiableEntity.Id), "anid".ToIdentifier()},
                {nameof(TestEntity.APropertyValue), "avalue"}
            });

            result.Id.Should().Be("anid".ToIdentifier());
            result.APropertyValue.Should().Be("avalue");
        }

        [Fact]
        public void WhenCreateValueObjectAndTypeNotExist_ThenThrows()
        {
            this.factory
                .Invoking(x => x
                    .RehydrateValueObject(typeof(TestValueObject), "avalue")).Should()
                .Throw<InvalidOperationException>()
                .WithMessageLike(Resources.DomainFactory_ValueObjectTypeNotFound);
        }

        [Fact]
        public void WhenCreateValueObjectAndExists_ThenReturnsEntityInstance()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(DomainFactorySpec).Assembly);

            var result = (TestValueObject) this.factory.RehydrateValueObject(typeof(TestValueObject), "avalue");

            result.APropertyValue.Should().Be("avalue");
        }
    }
}