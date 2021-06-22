using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public static class StringExtensions
    {
        public static string Format(this string value, params object[] arguments)
        {
            return string.Format(value, arguments);
        }

        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool EqualsOrdinal(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.Ordinal);
        }

        public static bool NotEqualsOrdinal(this string value, string other)
        {
            return !value.EqualsOrdinal(other);
        }

        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool NotEqualsIgnoreCase(this string value, string other)
        {
            return !value.EqualsIgnoreCase(other);
        }

        public static bool IsOneOf(this string source, params string[] values)
        {
            source.GuardAgainstNull(nameof(source));

            if (values.HasNone())
            {
                return false;
            }

            return values.Contains(source);
        }

        public static bool ToBool(this string value)
        {
            if (!value.HasValue())
            {
                return false;
            }
            return Convert.ToBoolean(value);
        }

        public static string TrimNonAlpha(this string value)
        {
            if (!value.HasValue())
            {
                return value;
            }

            var expression = new Regex(@"[^\p{L}]");
            return expression.Replace(value, string.Empty);
        }

        /// <summary>
        ///     Splits the given <see cref="value" /> into separate paragraphs on line endings (both Windows and Unix),
        ///     and removes empty lines.
        /// </summary>
        public static List<string> SplitParagraphs(this string value)
        {
            if (!value.HasValue())
            {
                return new List<string>();
            }

            var normalisedLineEndings = value
                .Replace("\r\n", "\r")
                .Replace("\n", "\r");
            return normalisedLineEndings
                .SafeSplit("\r")
                .ToList();
        }
    }
}