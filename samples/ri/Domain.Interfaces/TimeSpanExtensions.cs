using System;
using System.Xml;

namespace Domain.Interfaces
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan ToTimeSpan(this string value, TimeSpan? defaultValue = null)
        {
            if (!TimeSpan.TryParse(value, out var span))
            {
                var iso8601 = ToIso8601TimeSpan(value);
                if (iso8601 != TimeSpan.Zero)
                {
                    return iso8601;
                }

                if (defaultValue.HasValue)
                {
                    return defaultValue.Value;
                }

                return TimeSpan.Zero;
            }

            return span;
        }

        private static TimeSpan ToIso8601TimeSpan(this string value)
        {
            try
            {
                return XmlConvert.ToTimeSpan(value);
            }
            catch (FormatException)
            {
                return TimeSpan.Zero;
            }
        }
    }
}