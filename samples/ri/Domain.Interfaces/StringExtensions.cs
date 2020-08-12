using System;
using QueryAny.Primitives;

namespace Domain.Interfaces
{
    public static class StringExtensions
    {
        public static bool ToBool(this string value)
        {
            if (!value.HasValue())
            {
                return false;
            }
            return Convert.ToBoolean(value);
        }
    }
}