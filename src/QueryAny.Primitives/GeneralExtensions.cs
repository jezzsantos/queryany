using System;
using System.Diagnostics;

namespace QueryAny.Primitives
{
    [DebuggerStepThrough]
    public static class GeneralExtensions
    {
        public static void GuardAgainstNull(this object instance, string parameterName = null)
        {
            if (instance == null)
            {
                var ex = parameterName == null
                    ? new ArgumentNullException()
                    : new ArgumentNullException(parameterName);

                throw ex;
            }
        }

        public static void GuardAgainstNullOrEmpty(this string instance, string parameterName = null)
        {
            if (!instance.HasValue())
            {
                var ex = parameterName == null
                    ? new ArgumentNullException()
                    : new ArgumentNullException(parameterName);

                throw ex;
            }
        }
    }
}