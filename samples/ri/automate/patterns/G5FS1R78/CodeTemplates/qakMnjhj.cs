using ArchitectureTesting.Common;
using Xunit;

namespace {{DomainName | string.pascalplural}}ApiHost.UnitTests
{
    [Trait("Category", "Unit.Architecture"), Collection("ThisAssembly")]
    public class HorizontalLayersSpec : HorizontalLayersBaseSpec<Startup>
    {
        public HorizontalLayersSpec(ArchitectureSpecSetup<Startup> setup) : base(setup)
        {
        }
    }

    [Trait("Category", "Unit.Architecture"), Collection("ThisAssembly")]
    public class VerticalDomainsSpec : VerticalDomainsBaseSpec<Startup>
    {
        public VerticalDomainsSpec(ArchitectureSpecSetup<Startup> setup) : base(setup)
        {
        }
    }
}