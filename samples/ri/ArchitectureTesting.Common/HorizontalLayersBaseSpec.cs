using Xunit;

namespace ArchitectureTesting.Common
{
    public abstract class HorizontalLayersBaseSpec<TStartup> : IClassFixture<ArchitectureSpecSetup<TStartup>>
    {
        private readonly ArchitectureSpecSetup<TStartup> setup;

        protected HorizontalLayersBaseSpec(ArchitectureSpecSetup<TStartup> setup)
        {
            this.setup = setup;
        }

        [Fact]
        public void WhenDomainLayerDependsOnApplicationLayer_ThenThrows()
        {
            var violation = Types().That().Are(this.setup.DomainLayer)
                .Should().NotDependOnAny(this.setup.ApplicationLayer);

            violation.Check(this.setup.Architecture);
        }

        [Fact]
        public void WhenDomainLayerDependsOnInfrastructureLayer_ThenThrows()
        {
            var violation = Types().That().Are(this.setup.DomainLayer)
                .Should().NotDependOnAny(this.setup.InfrastructureLayer);

            violation.Check(this.setup.Architecture);
        }

        [Fact]
        public void WhenApplicationLayerDependsOnInfrastructureLayer_ThenThrows()
        {
            var violation = Types().That().Are(this.setup.ApplicationLayer)
                .Should().NotDependOnAny(this.setup.InfrastructureLayer);

            violation.Check(this.setup.Architecture);
        }
    }
}