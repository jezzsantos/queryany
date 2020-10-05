using System;
using System.Collections.Generic;
using CarsDomain.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class TimeSlot : ValueObjectBase<TimeSlot>
    {
        public TimeSlot(DateTime from, DateTime to)
        {
            from.GuardAgainstMinValue(nameof(from));
            to.GuardAgainstMinValue(nameof(to));
            to.GuardAgainstInvalid(dateTime => dateTime > from, nameof(from), Resources.TimeSlot_FromDateBeforeToDate);

            From = from;
            To = to;
        }

        public DateTime From { get; private set; }

        public DateTime To { get; private set; }

        public override void Rehydrate(string value)
        {
            var parts = RehydrateToList(value);
            From = parts[0].FromIso8601();
            To = parts[1].FromIso8601();
        }

        public static ValueObjectFactory<TimeSlot> Instantiate()
        {
            return (property, container) =>
            {
                var parts = RehydrateToList(property, false);
                return new TimeSlot(parts[0].FromIso8601(), parts[1].FromIso8601());
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {From.ToIso8601(), To.ToIso8601()};
        }
    }

    public static class TimeSlotExtensions
    {
        public static bool IsIntersecting(this TimeSlot first, TimeSlot second)
        {
            return second.IsEncompassing(first)
                   || first.IsOverlappedAtStartBy(second)
                   || first.IsOverlappedAtEndBy(second)
                   || first.IsUnderlappedBy(second);
        }

        public static bool IsOverlapping(this TimeSlot first, TimeSlot second)
        {
            return second.IsEncompassing(first)
                   || first.IsOverlappedAtStartBy(second)
                   || first.IsOverlappedAtEndBy(second);
        }

        private static bool IsEncompassing(this TimeSlot first, TimeSlot second)
        {
            return first.From <= second.From
                   && first.To >= second.To;
        }

        private static bool IsUnderlappedBy(this TimeSlot first, TimeSlot second)
        {
            return second.From >= first.From
                   && second.To <= first.To;
        }

        private static bool IsOverlappedAtStartBy(this TimeSlot first, TimeSlot second)
        {
            return second.From < first.From
                   && second.To > first.From
                   && second.To < first.To;
        }

        private static bool IsOverlappedAtEndBy(this TimeSlot first, TimeSlot second)
        {
            return second.From > first.From
                   && second.From < first.To
                   && second.To > first.To;
        }
    }
}