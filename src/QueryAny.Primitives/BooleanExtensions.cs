using System.Diagnostics;

namespace QueryAny.Primitives
{
    [DebuggerStepThrough]
    public static class BooleanExtensions
    {
        /// <summary>
        ///     Returns the lowercase representation of the <see cref="bool" />
        /// </summary>
        public static string ToLower(this bool value)
        {
            return value.ToString().ToLowerInvariant();
        }

        /// <summary>
        ///     Returns the uppercase representation of the <see cref="bool" />
        /// </summary>
        public static string ToUpper(this bool value)
        {
            return value.ToString().ToUpperInvariant();
        }
    }
}