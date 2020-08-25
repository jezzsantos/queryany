using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CarsDomain.Properties;
using Domain.Interfaces;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class Unavailabilities : IReadOnlyList<UnavailabilityEntity>
    {
        private readonly List<UnavailabilityEntity> internalUnavailability;

        public Unavailabilities()
        {
            this.internalUnavailability = new List<UnavailabilityEntity>();
        }

        public IEnumerator<UnavailabilityEntity> GetEnumerator()
        {
            return this.internalUnavailability.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => this.internalUnavailability.Count;

        public UnavailabilityEntity this[int index] => this.internalUnavailability[index];

        public void Add(UnavailabilityEntity unavailability)
        {
            var match = FindMatching(unavailability);
            if (match != null)
            {
                this.internalUnavailability.Remove(match);
            }

            this.internalUnavailability.Add(unavailability);
        }

        private UnavailabilityEntity FindMatching(UnavailabilityEntity unavailability)
        {
            return this.internalUnavailability
                .FirstOrDefault(u =>
                    Overlaps(u, unavailability) && !HasDifferentCause(u, unavailability));
        }

        public void EnsureValidState()
        {
            if (HasInvalidCauses())
            {
                throw new RuleViolationException(Resources.Unavailability_ReservationWithoutReference);
            }

            if (HasIncompatibleOverlaps())
            {
                throw new RuleViolationException(Resources.Unavailability_OverlappingSlot);
            }
        }

        private bool HasInvalidCauses()
        {
            return this.internalUnavailability.Any(current => current.CausedBy == UnavailabilityCausedBy.Reservation
                                                              && !current.CausedByReference.HasValue());
        }

        private bool HasIncompatibleOverlaps()
        {
            return this.internalUnavailability.Any(current =>
                this.internalUnavailability.Where(next => IsDifferentFrom(current, next))
                    .Any(next => InConflict(current, next)));
        }

        private static bool IsDifferentFrom(UnavailabilityEntity current, UnavailabilityEntity next)
        {
            return !next.Equals(current);
        }

        private static bool InConflict(UnavailabilityEntity current, UnavailabilityEntity next)
        {
            return Overlaps(current, next) && HasDifferentCause(current, next);
        }

        private static bool Overlaps(UnavailabilityEntity current, UnavailabilityEntity next)
        {
            return next.Slot.IsIntersecting(current.Slot);
        }

        private static bool HasDifferentCause(UnavailabilityEntity current, UnavailabilityEntity next)
        {
            return current.CausedByReference == null && next.CausedByReference == null &&
                   current.CausedBy != next.CausedBy
                   || current.CausedByReference != next.CausedByReference;
        }
    }
}