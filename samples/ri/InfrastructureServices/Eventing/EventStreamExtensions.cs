﻿using System.Collections.Generic;
using System.Linq;
using Application.Storage.Interfaces;

namespace InfrastructureServices.Eventing
{
    internal static class EventStreamExtensions
    {
        public static bool HasContiguousVersions(this List<EventStreamStateChangeEvent> events)
        {
            if (!events.Any())
            {
                return true;
            }

            static IEnumerable<long> GetRange(long start, long count)
            {
                for (long next = 0; next < count; next++)
                {
                    yield return start + next;
                }
            }

            var expectedRange = GetRange(events.First().Version, events.Count);
            return events.Select(e => e.Version).SequenceEqual(expectedRange);
        }
    }
}