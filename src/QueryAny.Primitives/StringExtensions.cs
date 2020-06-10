using System;
using System.Diagnostics;

namespace QueryAny.Primitives
{
    [DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        ///     Whether the specified value in not null and not empty
        /// </summary>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        ///     Whether the specified value is exactly equal to other value
        /// </summary>
        public static bool EqualsOrdinal(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Whether the specified value is not exactly equal to other value
        /// </summary>
        public static bool NotEqualsOrdinal(this string value, string other)
        {
            return !value.EqualsOrdinal(other);
        }

        /// <summary>
        ///     Whether the specified value is equal to other value (case-insensitive)
        /// </summary>
        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Whether the specified value is not equal to other value
        /// </summary>
        public static bool NotEqualsIgnoreCase(this string value, string other)
        {
            return !value.EqualsIgnoreCase(other);
        }

        public static string Format(this string value, params object[] args)
        {
            return string.Format(value, args);
        }
    }
}