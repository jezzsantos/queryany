using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class VehicleOwnerSpec
    {
        [TestMethod]
        public void WhenConstructedWithNullOwnerId_ThenThrows()
        {
            Action a = () => new VehicleOwner(null);

            a.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenConstructed_ThenOwnerIdAssigned()
        {
            var owner = new VehicleOwner("anownerid");

            owner.OwnerId.Should().Be("anownerid");
        }
    }
}