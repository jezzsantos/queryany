using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace QueryAny
{
    [DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        /// Whether the specified value in not null and not empty
        /// </summary>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Whether the specified value is exactly equal to other value
        /// </summary>
        public static bool EqualsOrdinal(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.Ordinal);
        }

        /// <summary>
        /// Whether the specified value is not exactly equal to other value
        /// </summary>
        public static bool NotEqualsOrdinal(this string value, string other)
        {
            return !value.EqualsOrdinal(other);
        }

        /// <summary>
        /// Whether the specified value is equal to other value (case-insensitive)
        /// </summary>
        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Whether the specified value is not equal to other value
        /// </summary>
        public static bool NotEqualsIgnoreCase(this string value, string other)
        {
            return !value.EqualsIgnoreCase(other);
        }

        /// <summary>
        /// Whether the <see cref="formattedString" /> has been formatted from the specified <see cref="formatString" />.
        /// </summary>
        /// <remarks>
        /// This function is useful for comparing two strings where the <see cref="formattedString" /> is the result of a String.Format operation on
        /// the <see cref="formatString" />, with one or more format substitutions.
        ///     For example: Calling this function with a string "My code is 5" and a resource string "My code is {0}" that contains one or more formatting arguments, return
        /// <c> true </c>
        /// </remarks>
        public static bool IsFormattedFrom(this string formattedString, string formatString)
        {
            var escapedPattern = formatString
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace(".", "\\.")
                .Replace("<", "\\<")
                .Replace(">", "\\>");

            var pattern = Regex.Replace(escapedPattern, @"\{\d+\}", ".*")
                .Replace(" ", @"\s");

            return new Regex(pattern).IsMatch(formattedString);
        }
    }
}
