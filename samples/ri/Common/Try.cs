using System;

namespace Common
{
    public static class Try
    {
        public static void Safely(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (ex is StackOverflowException
                    || ex is OutOfMemoryException)
                {
                    throw;
                }

                //Ignore exception!
            }
        }

        public static void ButRethrowIfFails<TException>(Action action, string exceptionMessage)
            where TException : Exception, new()
        {
            action.GuardAgainstNull(nameof(action));

            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (exceptionMessage.HasValue())
                {
                    throw (Exception) Activator.CreateInstance(typeof(TException), exceptionMessage, ex);
                }

                throw (Exception) Activator.CreateInstance(typeof(TException), exceptionMessage, ex);
            }
        }
    }
}