using System;
using System.Diagnostics;

namespace QueryAny.Primitives
{
    [DebuggerStepThrough]
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Whether the specified date a value assigned to it
        /// </summary>
        /// <remarks>
        ///     The specified date may be a UTC datetime or not, either way this function determines whether the date is NOT close
        ///     to the <see cref="DateTime.MinValue" />
        /// </remarks>
        public static bool HasValue(this DateTime current)
        {
            if (current.Kind == DateTimeKind.Local)
            {
                return current != DateTime.MinValue.ToLocalTime()
                       && current != DateTime.MinValue;
            }

            return current != DateTime.MinValue;
        }

        /// <summary>
        ///     Returns the ISO8601 representation of the specified <see cref="DateTimeOffset" /> value.
        /// </summary>
        public static string ToIso8601(this DateTimeOffset dateTimeOffset)
        {
            var utcDateTime = dateTimeOffset.ToUniversalTime();

            return utcDateTime.ToString("O");
        }

        /// <summary>
        ///     Returns the ISO8601 representation of the specified <see cref="DateTime" /> value.
        /// </summary>
        public static string ToIso8601(this DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind != DateTimeKind.Utc
                ? dateTime
                : dateTime.ToUniversalTime();

            return utcDateTime.ToString("O");
        }

        /// <summary>
        ///     Returns the UTC <see cref="DateTime" /> for the specified time in ISO8601
        /// </summary>
        public static DateTime FromIso8601(this string value)
        {
            if (!value.HasValue())
            {
                return DateTime.MinValue;
            }

            var dateTime = DateTime.ParseExact(value, "O", null);

            return dateTime.HasValue()
                ? dateTime.ToUniversalTime()
                : DateTime.MinValue;
        }
    }
}