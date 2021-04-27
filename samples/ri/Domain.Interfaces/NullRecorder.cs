using System;

namespace Domain.Interfaces
{
    public class NullRecorder : IRecorder
    {
        public static readonly IRecorder Instance = new NullRecorder();

        protected NullRecorder()
        {
        }

        public virtual void TraceDebug(string message, params object[] args)
        {
        }

        public virtual void TraceError(Exception exception, string message, params object[] args)
        {
        }

        public virtual void TraceError(string message, params object[] args)
        {
        }

        public virtual void TraceInformation(string message, params object[] args)
        {
        }

        public virtual void TraceWarning(Exception exception, string message, params object[] args)
        {
        }

        public virtual void TraceWarning(string message, params object[] args)
        {
        }

        public virtual void Crash(CrashLevel level, Exception exception, string caller)
        {
        }

        public virtual void Crash(CrashLevel level, Exception ex, string message, string caller, params object[] args)
        {
        }

        public void Audit(string eventId, string caller, string message, params object[] args)
        {
        }

        public void Count(string eventId)
        {
        }
    }
}