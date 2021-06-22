using System;
using System.Collections.Generic;
using System.Linq;
using ArchUnitNET.Domain.Extensions;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ArchitectureTesting.Common
{
    public abstract class VerticalDomainsBaseSpec<TStartup> : IClassFixture<ArchitectureSpecSetup<TStartup>>
    {
        private const string DomainSpecificDomainProjectSuffix = "Domain";
        private const string DomainSpecificApplicationProjectSuffix = "Application";
        private const string DomainSpecificApiProjectSuffix = "Api";
        private const string DomainSpecificStorageProjectSuffix = "Storage";
        private readonly List<string> allDomains;
        private readonly Func<string, string, string> domainSpecificProjectNamespaceFormat =
            (domainName, prefix) => $@"^{domainName}{prefix}((\.[\w]+)*)$";
        private readonly Func<string, string, string> otherDomainSpecificProjectsNamespaceFormat =
            (domainName, prefix) =>
                $@"^(((?!{domainName})[\w]+){prefix}((\.[\w]+)*))$";
        private readonly ArchitectureSpecSetup<TStartup> setup;

        protected VerticalDomainsBaseSpec(ArchitectureSpecSetup<TStartup> setup)
        {
            this.setup = setup;

            this.allDomains = this.setup.Architecture.Namespaces
                .Where(ns => ns.NameEndsWith(DomainSpecificDomainProjectSuffix))
                .Select(ns =>
                    ns.Name.Substring(0, ns.Name.IndexOf(DomainSpecificDomainProjectSuffix, StringComparison.Ordinal)))
                .Distinct()
                .ToList();
        }

        [Fact]
        public void WhenAnyDomainDependsOnAnotherDomains_ThenThrows()
        {
            this.allDomains.ForEach(domainName =>
            {
                var domainTypes = Types().That()
                    .ResideInNamespace(
                        this.domainSpecificProjectNamespaceFormat(domainName, DomainSpecificDomainProjectSuffix), true)
                    .As($"{domainName}Domain");
                var allOtherDomains = Types().That()
                    .ResideInNamespace(
                        this.otherDomainSpecificProjectsNamespaceFormat(domainName, DomainSpecificDomainProjectSuffix),
                        true)
                    .As("Other Domains' Domain types");

                var violation = Types().That().Are(domainTypes)
                    .Should().NotDependOnAny(allOtherDomains);

                violation.Check(this.setup.Architecture);
            });
        }

        [Fact]
        public void WhenAnyDomainApplicationDependsOnAnotherDomains_ThenThrows()
        {
            this.allDomains.ForEach(domainName =>
            {
                var domainTypes = Types().That()
                    .ResideInNamespace(
                        this.domainSpecificProjectNamespaceFormat(domainName, DomainSpecificApplicationProjectSuffix),
                        true)
                    .As($"{domainName}Application");
                var allOtherDomains = Types().That()
                    .ResideInNamespace(
                        this.otherDomainSpecificProjectsNamespaceFormat(domainName,
                            DomainSpecificApplicationProjectSuffix), true)
                    .As("Other Domains' Application types");

                var violation = Types().That().Are(domainTypes)
                    .Should().NotDependOnAny(allOtherDomains);

                violation.Check(this.setup.Architecture);
            });
        }

        [Fact]
        public void WhenAnyDomainApiDependsOnAnotherDomains_ThenThrows()
        {
            this.allDomains.ForEach(domainName =>
            {
                var domainTypes = Types().That()
                    .ResideInNamespace(
                        this.domainSpecificProjectNamespaceFormat(domainName, DomainSpecificApiProjectSuffix), true)
                    .As($"{domainName}Api");
                var allOtherDomains = Types().That()
                    .ResideInNamespace(
                        this.otherDomainSpecificProjectsNamespaceFormat(domainName, DomainSpecificApiProjectSuffix),
                        true)
                    .As("Other Domains' Api types");

                var violation = Types().That().Are(domainTypes)
                    .Should().NotDependOnAny(allOtherDomains);

                violation.Check(this.setup.Architecture);
            });
        }

        [Fact]
        public void WhenAnyDomainStorageDependsOnAnotherDomains_ThenThrows()
        {
            this.allDomains.ForEach(domainName =>
            {
                var domainTypes = Types().That()
                    .ResideInNamespace(
                        this.domainSpecificProjectNamespaceFormat(domainName, DomainSpecificStorageProjectSuffix), true)
                    .As($"{domainName}Storage");
                var allOtherDomains = Types().That()
                    .ResideInNamespace(
                        this.otherDomainSpecificProjectsNamespaceFormat(domainName, DomainSpecificStorageProjectSuffix),
                        true)
                    .As("Other Domains' Storage types");

                var violation = Types().That().Are(domainTypes)
                    .Should().NotDependOnAny(allOtherDomains);

                violation.Check(this.setup.Architecture);
            });
        }
    }
}