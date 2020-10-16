using System;
using System.Linq;
using CarsDomain.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CarsDomain.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class CarEntitySpec
    {
        private CarEntity entity;
        private Mock<IIdentifierFactory> identifierFactory;
        private Mock<ILogger> logger;

        [TestInitialize]
        public void Initialize()
        {
            this.logger = new Mock<ILogger>();
            this.identifierFactory = new Mock<IIdentifierFactory>();
            var entityCount = 0;
            this.identifierFactory.Setup(f => f.Create(It.IsAny<IIdentifiableEntity>()))
                .Returns((IIdentifiableEntity e) =>
                {
                    if (e is UnavailabilityEntity)
                    {
                        return $"anunavailbilityid{++entityCount}".ToIdentifier();
                    }
                    return "anid".ToIdentifier();
                });
            this.entity = new CarEntity(this.logger.Object, this.identifierFactory.Object);
        }

        [TestMethod]
        public void WhenSetManufacturer_ThenManufactured()
        {
            var manufacturer =
                new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0], Manufacturer.Models[0]);
            this.entity.SetManufacturer(manufacturer);

            this.entity.Manufacturer.Should()
                .Be(manufacturer);
            this.entity.Events[1].Should().BeOfType<Events.Car.ManufacturerChanged>();
        }

        [TestMethod]
        public void WhenSetOwnership_ThenOwnedAndManaged()
        {
            var owner = new VehicleOwner("anownerid");
            this.entity.SetOwnership(owner);

            this.entity.Owner.Should().Be(new VehicleOwner(owner.OwnerId));
            this.entity.Managers.Managers.Single().Should().Be("anownerid".ToIdentifier());
            this.entity.Events[1].Should().BeOfType<Events.Car.OwnershipChanged>();
        }

        [TestMethod]
        public void WhenRegistered_ThenRegistered()
        {
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));

            this.entity.Plate.Should().Be(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));
            this.entity.Events[1].Should().BeOfType<Events.Car.RegistrationChanged>();
        }

        [TestMethod]
        public void WhenOfflineAndNotManufactured_ThenThrows()
        {
            this.entity.SetOwnership(new VehicleOwner("anownerid"));
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));

            this.entity.Invoking(x => x.Offline(new TimeSlot(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(1))))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.CarEntity_NotManufactured);
        }

        [TestMethod]
        public void WhenOfflineAndNotOwned_ThenThrows()
        {
            this.entity.SetManufacturer(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0],
                Manufacturer.Models[0]));
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));

            this.entity.Invoking(x => x.Offline(new TimeSlot(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(1))))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.CarEntity_NotOwned);
        }

        [TestMethod]
        public void WhenOfflineAndNotRegistered_ThenThrows()
        {
            this.entity.SetManufacturer(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0],
                Manufacturer.Models[0]));
            this.entity.SetOwnership(new VehicleOwner("anownerid"));

            this.entity.Invoking(x => x.Offline(new TimeSlot(DateTime.UtcNow, DateTime.UtcNow.AddSeconds(1))))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.CarEntity_NotRegistered);
        }

        [TestMethod]
        public void WhenOffline_ThenUnavailable()
        {
            var from = DateTime.UtcNow.AddDays(1);
            var to = from.AddDays(1);
            var slot = new TimeSlot(from, to);
            SetupUnavailabilityContext();

            this.entity.Offline(slot);

            this.entity.Unavailabilities.Count.Should().Be(1);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot);
            this.entity.Unavailabilities[0].CausedBy.Should().Be(UnavailabilityCausedBy.Offline);
            this.entity.Unavailabilities[0].CausedByReference.Should().BeNull();
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
            this.entity.Events[4].As<Events.Car.UnavailabilitySlotAdded>().EntityId.Should().Be("anunavailbilityid1");
        }

        [TestMethod]
        public void WhenAddUnavailableAndNotExist_ThenCreatesUnavailability()
        {
            var datum = DateTime.UtcNow;
            var slot = new TimeSlot(datum, datum.AddMinutes(1));
            SetupUnavailabilityContext();

            this.entity.AddUnavailability(slot, UnavailabilityCausedBy.Other, null);

            this.entity.Unavailabilities.Count.Should().Be(1);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot);
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
        }

        [TestMethod]
        public void WhenAddUnavailableAndCausedByReservationButNoReference_ThenThrows()
        {
            var datum = DateTime.UtcNow;
            var slot = new TimeSlot(datum, datum.AddMinutes(1));
            SetupUnavailabilityContext();

            this.entity
                .Invoking(x =>
                    x.AddUnavailability(slot, UnavailabilityCausedBy.Reservation, null))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.Unavailability_ReservationWithoutReference);
        }

        [TestMethod]
        public void WhenAddUnavailableWithIntersectingSlotWithSameCauseNoReference_ThenReplacesEntity()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum, datum.AddMinutes(5));
            SetupUnavailabilityContext();

            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, null);
            this.entity.AddUnavailability(slot2, UnavailabilityCausedBy.Other, null);

            this.entity.Unavailabilities.Count.Should().Be(1);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot2);
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
            this.entity.Events[5].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
        }

        [TestMethod]
        public void WhenAddUnavailableWithIntersectingSlotWithSameCauseSameReference_ThenReplacesEntity()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum, datum.AddMinutes(5));
            SetupUnavailabilityContext();

            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, "aref");
            this.entity.AddUnavailability(slot2, UnavailabilityCausedBy.Other, "aref");

            this.entity.Unavailabilities.Count.Should().Be(1);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot2);
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
            this.entity.Events[5].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
        }

        [TestMethod]
        public void WhenAddUnavailableWithIntersectingSlotWithDifferentCauseNoReference_ThenThrows()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum, datum.AddMinutes(5));
            SetupUnavailabilityContext();
            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, null);

            this.entity
                .Invoking(x => x.AddUnavailability(slot2, UnavailabilityCausedBy.Offline, null))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.Unavailability_OverlappingSlot);
        }

        [TestMethod]
        public void WhenAddUnavailableWithIntersectingSlotWithSameCauseDifferentReference_ThenThrows()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum, datum.AddMinutes(5));
            SetupUnavailabilityContext();
            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, "aref1");

            this.entity
                .Invoking(x => x.AddUnavailability(slot2, UnavailabilityCausedBy.Other, "aref2"))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.Unavailability_OverlappingSlot);
        }

        [TestMethod]
        public void WhenAddUnavailableWithIntersectingSlotWithDifferentCauseDifferentReference_ThenThrows()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum, datum.AddMinutes(5));
            SetupUnavailabilityContext();
            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, "aref1");

            this.entity
                .Invoking(x => x.AddUnavailability(slot2, UnavailabilityCausedBy.Offline, "aref2"))
                .Should().Throw<RuleViolationException>()
                .WithMessageLike(Resources.Unavailability_OverlappingSlot);
        }

        [TestMethod]
        public void WhenAddUnavailableWithNotIntersectingSlotSameCause_ThenAddsUnavailability()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum.AddMinutes(5), datum.AddMinutes(10));
            SetupUnavailabilityContext();

            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, null);
            this.entity.AddUnavailability(slot2, UnavailabilityCausedBy.Other, null);

            this.entity.Unavailabilities.Count.Should().Be(2);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot1);
            this.entity.Unavailabilities[1].Slot.Should().Be(slot2);
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
            this.entity.Events[5].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
        }

        [TestMethod]
        public void WhenAddUnavailableWithNotIntersectingSlotDifferentCause_ThenAddsUnavailability()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum.AddMinutes(5), datum.AddMinutes(10));
            SetupUnavailabilityContext();

            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, null);
            this.entity.AddUnavailability(slot2, UnavailabilityCausedBy.Offline, null);

            this.entity.Unavailabilities.Count.Should().Be(2);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot1);
            this.entity.Unavailabilities[1].Slot.Should().Be(slot2);
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
            this.entity.Events[5].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
        }

        [TestMethod]
        public void
            WhenAddUnavailableWithNotIntersectingSlotDifferentCauseAndDifferentReference_ThenAddsUnavailability()
        {
            var datum = DateTime.UtcNow;
            var slot1 = new TimeSlot(datum, datum.AddMinutes(1));
            var slot2 = new TimeSlot(datum.AddMinutes(5), datum.AddMinutes(10));
            SetupUnavailabilityContext();

            this.entity.AddUnavailability(slot1, UnavailabilityCausedBy.Other, "aref1");
            this.entity.AddUnavailability(slot2, UnavailabilityCausedBy.Offline, "aref2");

            this.entity.Unavailabilities.Count.Should().Be(2);
            this.entity.Unavailabilities[0].Slot.Should().Be(slot1);
            this.entity.Unavailabilities[1].Slot.Should().Be(slot2);
            this.entity.Events[4].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
            this.entity.Events[5].Should().BeOfType<Events.Car.UnavailabilitySlotAdded>();
        }

        private void SetupUnavailabilityContext()
        {
            this.entity.SetManufacturer(new Manufacturer(Manufacturer.MinYear + 1, Manufacturer.Makes[0],
                Manufacturer.Models[0]));
            this.entity.SetOwnership(new VehicleOwner("anownerid"));
            this.entity.Register(new LicensePlate(LicensePlate.Jurisdictions[0], "anumber"));
        }
    }
}