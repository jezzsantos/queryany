using System;
using System.Linq;
using Api.Interfaces.ServiceOperations.{{DomainName | string.pascalplural}};
using Application.Storage.Interfaces;
using {{DomainName | string.pascalplural}}Application.ReadModels;
using {{DomainName | string.pascalplural}}Domain;
using Domain.Interfaces;
using FluentAssertions;
using IntegrationTesting.Common;
using Xunit;
using {{DomainName | string.pascalsingular}} = Application.Interfaces.Resources.{{DomainName | string.pascalsingular}};

namespace {{DomainName | string.pascalplural}}Api.IntegrationTests
{
    [Trait("Category", "Integration.Web"), Collection("ThisAssembly")]
    public class {{DomainName | string.pascalplural}}ApiSpec : IClassFixture<ApiSpecSetup<TestStartup>>
    {
        private readonly ApiSpecSetup<TestStartup> setup;

        public {{DomainName | string.pascalplural}}ApiSpec(ApiSpecSetup<TestStartup> setup)
        {
            this.setup = setup;
            this.setup.Resolve<IQueryStorage<{{DomainName | string.pascalplural}}Application.ReadModels.{{DomainName | string.pascalsingular}}>>().DestroyAll();
            this.setup.Resolve<IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity>>().DestroyAll();
        }
    }
}