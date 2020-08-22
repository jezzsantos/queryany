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

            var isMatch = format.IsMatchedWith(value);
            if (!isMatch)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void GuardAgainstInvalid(this string value, Func<string, bool> validator, string parameterName)
        {
            validator.GuardAgainstNull(nameof(validator));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var isValid = validator(value);
            if (!isValid)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}