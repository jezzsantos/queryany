using System;
using System.Collections.Generic;
using System.Linq;
using Api.Common.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Api.Common.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class DomainFactorySpec
    {
        private Mock<IDependencyContainer> dependencyContainer;
        private DomainFactory factory;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            this.identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier);
            this.dependencyContainer = new Mock<IDependencyContainer>();
            this.dependencyContainer.Setup(dc => dc.Resolve<ILogger>())
                .Returns(this.logger.Object);
            this.dependencyContainer.Setup(dc => dc.Resolve<IIdentifierFactory>())
                .Returns(this.identifierFactory.Object);

            this.factory = new DomainFactory(this.dependencyContainer.Object);
        }

        [TestMethod]
        public void WhenRegisterAndNoAssemblies_ThenRegistersNone()
        {
            this.factory.RegisterDomainTypesFromAssemblies();

            this.factory.EntityFactories.Count.Should().Be(0);
            this.factory.ValueObjectFactories.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenRegisterAndAssemblyContainsNoFactories_ThenRegistersNone()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(Exactly).Assembly);

            this.factory.EntityFactories.Count.Should().Be(0);
            this.factory.ValueObjectFactories.Count.Should().Be(0);
        }

        [TestMethod]
        public void WhenRegisterAndAssemblyContainsFactories_ThenRegistersFactories()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(DomainFactorySpec).Assembly);

            this.factory.EntityFactories.Count.Should().Be(1);
            this.factory.EntityFactories.First().Key.Should().Be(typeof(TestEntity));
            this.factory.ValueObjectFactories.Count.Should().Be(1);
            this.factory.ValueObjectFactories.First().Key.Should().Be(typeof(TestValueObject));
        }

        [TestMethod]
        public void WhenCreateEntityAndTypeNotExist_ThenThrows()
        {
            this.factory
                .Invoking(x => x
                    .RehydrateEntity(typeof(TestEntity), new Dictionary<string, object>())).Should()
                .Throw<InvalidOperationException>()
                .WithMessageLike(Resources.DomainFactory_EntityTypeNotFound);
        }

        [TestMethod]
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

        [TestMethod]
        public void WhenCreateValueObjectAndTypeNotExist_ThenThrows()
        {
            this.factory
                .Invoking(x => x
                    .RehydrateValueObject(typeof(TestValueObject), "avalue")).Should()
                .Throw<InvalidOperationException>()
                .WithMessageLike(Resources.DomainFactory_ValueObjectTypeNotFound);
        }

        [TestMethod]
        public void WhenCreateValueObjectAndExists_ThenReturnsEntityInstance()
        {
            this.factory.RegisterDomainTypesFromAssemblies(typeof(DomainFactorySpec).Assembly);

            var result = (TestValueObject) this.factory.RehydrateValueObject(typeof(TestValueObject), "avalue");

            result.APropertyValue.Should().Be("avalue");
        }
    }
}