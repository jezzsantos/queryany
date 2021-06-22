using System;
using Common.Properties;
using TimeZoneConverter;

namespace Common
{
    public static class DateTimeExtensions
    {
        public static bool HasValue(this DateTime current)
        {
            if (current.Kind == DateTimeKind.Local)
            {
                return current != DateTime.MinValue.ToLocalTime()
                       && current != DateTime.MinValue;
            }

            return current != DateTime.MinValue;
        }

        public static bool HasValue(this DateTime? datum)
        {
            if (!datum.HasValue)
            {
                return false;
            }

            return datum.Value.HasValue();
        }

        public static string ToIso8601(this DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = dateTimeOffset.ToUniversalTime();

            return utcDateTime.ToString("O");
        }

        public static string ToIso8601(this DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind != DateTimeKind.Utc
                ? dateTime
                : dateTime.ToUniversalTime();

            return utcDateTime.ToString("O");
        }

        public static DateTime FromIso8601(this string value)
        {
            if (!value.HasValue())
            {
                return DateTime.MinValue;
            }

            var dateTime = DateTime.Parse(value);

            return dateTime.HasValue()
                ? dateTime.ToUniversalTime()
                : DateTime.MinValue;
        }

        public static DateTime ToLocalTime(this DateTime utcDate, string ianaTimezone)
        {
            utcDate.GuardAgainstInvalid(d => d != DateTime.MinValue, nameof(utcDate));
            utcDate.GuardAgainstInvalid(d => d.Kind == DateTimeKind.Utc, nameof(utcDate),
                Resources.DateTimeExtensions_DateTimeIsNotUtc);
            ianaTimezone.GuardAgainstNull(nameof(ianaTimezone));

            if (!TZConvert.TryGetTimeZoneInfo(ianaTimezone, out var timeZoneInfo))
            {
                throw new ArgumentOutOfRangeException(Resources.DateTimeExtensions_InvalidTimezone);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDate, timeZoneInfo);
        }

        public static string ToIana(this TimeZoneInfo tz)
        {
            if (OperatingSystem.IsMacOS())
            {
                return tz.Id;
            }

            return TZConvert.WindowsToIana(tz.Id);
        }

        public static bool IsAfter(this DateTime datum, DateTime compareTo)
        {
            return datum > compareTo;
        }

        public static bool IsBefore(this DateTime datum, DateTime compareTo)
        {
            return datum < compareTo;
        }

        public static bool IsNear(this DateTime datum, DateTime compareTo, int withinMilliseconds = 50)
        {
            if (datum.Equals(compareTo))
            {
                return true;
            }

            return datum.AddMilliseconds(withinMilliseconds) >= compareTo
                   && datum.AddMilliseconds(-withinMilliseconds) <= compareTo;
        }

        public static bool IsNear(this DateTime datum, DateTime compareTo, TimeSpan within)
        {
            return datum.IsNear(compareTo, (int) within.TotalMilliseconds);
        }
    }
}