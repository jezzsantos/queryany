using System;
using Common;

namespace Api.Common
{
    public class NullCrashReporter : ICrashReporter
    {
        public void Crash(CrashLevel level, Exception exception, string caller, string message, object[] args)
        {
        }
    }
}