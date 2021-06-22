using Domain.Interfaces.Entities;
using FluentAssertions;
using Xunit;

namespace CarsDomain.UnitTests
{
    [Trait("Category", "Unit")]
    public class VehicleManagersSpec
    {
        private readonly VehicleManagers managers;

        public VehicleManagersSpec()
        {
            this.managers = new VehicleManagers();
        }

        [Fact]
        public void WhenConstructed_ThenHasNoManagers()
        {
            this.managers.Managers.Should().BeEmpty();
        }

        [Fact]
        public void WhenAddAndManagerNotExist_ThenAddsManager()
        {
            this.managers.Add("amanagerid".ToIdentifier());

            this.managers.Managers.Count.Should().Be(1);
            this.managers.Managers[0].Should().Be("amanagerid".ToIdentifier());
        }

        [Fact]
        public void WhenAddAndManagerAndExist_ThenDoesNotManager()
        {
            this.managers.Add("amanagerid".ToIdentifier());
            this.managers.Add("amanagerid".ToIdentifier());
            this.managers.Add("amanagerid".ToIdentifier());

            this.managers.Managers.Count.Should().Be(1);
            this.managers.Managers[0].Should().Be("amanagerid".ToIdentifier());
        }
    }
}