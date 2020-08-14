using System;
using QueryAny.Primitives;

namespace Domain.Interfaces
{
    public static class GuardExtensions
    {
        public static void GuardAgainstInvalid(this string value, ValidationFormat format, string parameterName)
        {
            format.GuardAgainstNull(nameof(format));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var match = format.IsMatchedWith(value);
            if (!match)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}