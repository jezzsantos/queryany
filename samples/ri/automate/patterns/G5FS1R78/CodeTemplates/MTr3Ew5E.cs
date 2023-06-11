using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Interfaces.Resources;
using ApplicationServices.Interfaces;
using {{DomainName | string.pascalplural}}Application.Storage;
using {{DomainName | string.pascalplural}}Domain;
using Common;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using {{DomainName | string.pascalsingular}} = {{DomainName | string.pascalplural}}Application.ReadModels.{{DomainName | string.pascalsingular}};

namespace {{DomainName | string.pascalplural}}Application.UnitTests
{
    [Trait("Category", "Unit")]
    public class {{DomainName | string.pascalplural}}ApplicationSpec
    {
        private readonly Mock<ICurrentCaller> caller;
        private readonly {{DomainName | string.pascalplural}}Application {{DomainName | string.camelplural}}Application;
        private readonly Mock<IIdentifierFactory> idFactory;
        private readonly Mock<IRecorder> recorder;
        private readonly Mock<I{{DomainName | string.pascalsingular}}Storage> storage;

        public {{DomainName | string.pascalplural}}ApplicationSpec()
        {
            this.recorder = new Mock<IRecorder>();
            this.idFactory = new Mock<IIdentifierFactory>();
            this.idFactory.Setup(idf => idf.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns("anid".ToIdentifier());
            this.idFactory.Setup(idf => idf.IsValid(It.IsAny<Identifier>()))
                .Returns(true);
            this.storage = new Mock<I{{DomainName | string.pascalsingular}}Storage>();
            this.caller = new Mock<ICurrentCaller>();
            this.caller.Setup(c => c.Id).Returns("acallerid");
            this.{{DomainName | string.camelplural}}Application = new {{DomainName | string.pascalplural}}Application(this.recorder.Object, this.idFactory.Object, this.storage.Object);
        }
    }
}