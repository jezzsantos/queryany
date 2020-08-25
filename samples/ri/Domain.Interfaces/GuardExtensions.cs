using System;
using QueryAny.Primitives;

namespace Domain.Interfaces
{
    public static class GuardExtensions
    {
        public static void GuardAgainstInvalid(this string value, ValidationFormat format, string parameterName,
            string errorMessage = null)
        {
            format.GuardAgainstNull(nameof(format));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var isMatch = format.IsMatchedWith(value);
            if (!isMatch)
            {
                if (errorMessage.HasValue())
                {
                    throw new ArgumentOutOfRangeException(parameterName, errorMessage);
                }
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void GuardAgainstInvalid<TValue>(this TValue value, Func<TValue, bool> validator,
            string parameterName, string errorMessage = null)
        {
            validator.GuardAgainstNull(nameof(validator));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var isValid = validator(value);
            if (!isValid)
            {
                if (errorMessage.HasValue())
                {
                    throw new ArgumentOutOfRangeException(parameterName, errorMessage);
                }
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void GuardAgainstMinValue(this DateTime value, string parameterName)
        {
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            if (!value.HasValue())
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}