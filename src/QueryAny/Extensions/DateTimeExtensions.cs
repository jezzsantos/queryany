using System;
using System.Diagnostics;

namespace QueryAny.Extensions
{
    [DebuggerStepThrough]
    internal static class DateTimeExtensions
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
    }
}