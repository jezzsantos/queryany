using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryAny.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Safe<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }
        public static string Join<T>(this IEnumerable<T> values)
        {
            return values.Join(",");
        }

        public static string Join<T>(this IEnumerable<T> values, string seperator)
        {
            var stringBuilder = new StringBuilder();
            foreach (var value in values)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(seperator);
                }
                stringBuilder.Append(value);
            }
            return stringBuilder.ToString();
        }

        public static string[] SafeSplit(this string value, string[] delimiters, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (!value.HasValue())
            {
                return new string[]
                {
                };
            }

            return value.Split(delimiters, options);
        }

        public static string[] SafeSplit(this string value, string delimiter, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return value.SafeSplit(new[]
            {
                delimiter
            }, options);
        }
    }
}
