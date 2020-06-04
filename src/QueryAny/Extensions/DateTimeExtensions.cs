using System;
using System.Diagnostics;

namespace QueryAny
{
    [DebuggerStepThrough]
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Whether the specified date a value assigned to it
        /// </summary>
        /// <remarks>
        /// The specified date may be a UTC datetime or not, either way this function determines whether the date is NOT close to the
        /// <see cref="DateTime.MinValue" />
        /// </remarks>
        public static bool HasValue(this DateTime current)
        {
            if ((current.Kind == DateTimeKind.Local
                 || current.Kind == DateTimeKind.Unspecified)
                && (current.Equals(DateTime.MinValue)))
            {
                return false;
            }

            return !(current.Equals(DateTime.MinValue.ToUniversalTime())
                     || current.Equals(DateTime.MinValue));
        }
    }
}
