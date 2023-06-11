using System;
using System.Linq;
using {{DomainName | string.pascalplural}}Domain.Properties;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using UnitTesting.Common;
using Xunit;

namespace {{DomainName | string.pascalplural}}Domain.UnitTests
{
    [Trait("Category", "Unit")]
    public class {{DomainName | string.pascalsingular}}EntitySpec
    {
        private readonly {{DomainName | string.pascalsingular}}Entity entity;

        public {{DomainName | string.pascalsingular}}EntitySpec()
        {
            var recorder = new Mock<IRecorder>();
            var identifierFactory = new Mock<IIdentifierFactory>();
            identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns((IIdentifiableEntity e) => "anid".ToIdentifier());
            this.entity = new {{DomainName | string.pascalsingular}}Entity(recorder.Object, identifierFactory.Object);
        }
    }
}