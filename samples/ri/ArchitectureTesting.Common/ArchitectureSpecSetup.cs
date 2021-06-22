using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Syntax.Elements.Types;
using ArchUnitNET.Loader;

namespace ArchitectureTesting.Common
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ArchitectureSpecSetup<TStartup>
    {
        private const string DomainSpecificProjectNamespaces = @"^[\w]+Domain[\w\.]*$";
        private const string DomainCommonProjectNamespaces = @"^Domain[\w\.]*$";
        private const string ExternalCommonProjectsNamespaces = @"^External[\w\.]*$";
        private const string DomainSpecificApiProjectNamespaces = @"^[\w]+Api[\w\.]*$";
        private const string DomainSpecificStorageProjectNamespaces = @"^[\w]+Storage[\w\.]*$";
        private const string InfrastructureCommonProjectsNamespaces = @"^Infrastructure[\w\.]*$";
        private const string DomainSpecificApplicationProjectNamespaces = @"^[\w]+Application[\w\.]*$";
        private const string ApplicationCommonProjectNamespaces = @"^Application[\w\.]*$";
        private const string StorageCommonProjectNamespaces = @"^Storage[\w\.]*$";
        private const string ApiCommonProjectNamespaces = @"^Api[\w\.]*$";

        public ArchitectureSpecSetup()
        {
            Architecture = new ArchLoader()
                .LoadAssemblyIncludingDependencies(typeof(TStartup).Assembly)
                .Build();
            InfrastructureLayer = ArchRuleDefinition.Types().That()
                .ResideInNamespace(DomainSpecificApiProjectNamespaces, true)
                .Or().ResideInNamespace(ApiCommonProjectNamespaces, true)
                .Or().ResideInNamespace(DomainSpecificStorageProjectNamespaces, true)
                .Or().ResideInNamespace(StorageCommonProjectNamespaces, true)
                .Or().ResideInNamespace(InfrastructureCommonProjectsNamespaces, true)
                .Or().ResideInNamespace(ExternalCommonProjectsNamespaces, true)
                .As("Infrastructure Layer");
            ApplicationLayer = ArchRuleDefinition.Types().That()
                .ResideInNamespace(DomainSpecificApplicationProjectNamespaces, true)
                .Or().ResideInNamespace(ApplicationCommonProjectNamespaces, true)
                .As("Application Layer");
            DomainLayer = ArchRuleDefinition.Types().That()
                .ResideInNamespace(DomainSpecificProjectNamespaces, true)
                .Or().ResideInNamespace(DomainCommonProjectNamespaces, true)
                .As("Domain Layer");
        }

        public GivenTypesConjunctionWithDescription DomainLayer { get; }

        public GivenTypesConjunctionWithDescription ApplicationLayer { get; }

        public GivenTypesConjunctionWithDescription InfrastructureLayer { get; }

        public Architecture Architecture { get; }
    }
}