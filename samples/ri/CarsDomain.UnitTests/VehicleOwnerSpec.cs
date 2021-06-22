using System;
using FluentAssertions;
using Xunit;

namespace CarsDomain.UnitTests
{
    [Trait("Category", "Unit")]
    public class VehicleOwnerSpec
    {
        [Fact]
        public void WhenConstructedWithNullOwnerId_ThenThrows()
        {
            Action a = () => new VehicleOwner(null);

            a.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenConstructed_ThenOwnerIdAssigned()
        {
            var owner = new VehicleOwner("anownerid");

            owner.OwnerId.Should().Be("anownerid");
        }
    }
}