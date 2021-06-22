using System;
using Common;

namespace Domain.Interfaces
{
    public static class GuardExtensions
    {
        public static void GuardAgainstInvalid<TValue>(this TValue value, ValidationFormat<TValue> format,
            string parameterName,
            string errorMessage = null)
        {
            format.GuardAgainstNull(nameof(format));
            parameterName.GuardAgainstNullOrEmpty(nameof(parameterName));

            var isMatch = format.Matches(value);
            if (!isMatch)
            {
                if (errorMessage.HasValue())
                {
                    throw new ArgumentOutOfRangeException(parameterName, errorMessage);
                }
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}