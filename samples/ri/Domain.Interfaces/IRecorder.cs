using System;

namespace Domain.Interfaces
{
    public interface IRecorder
    {
        void TraceDebug(string message, params object[] args);

        void TraceInformation(string message, params object[] args);

        void TraceWarning(Exception exception, string message, params object[] args);

        void TraceWarning(string message, params object[] args);

        void TraceError(Exception exception, string message, params object[] args);

        void TraceError(string message, params object[] args);

        void Crash(CrashLevel level, Exception exception, string caller);

        void Crash(CrashLevel level, Exception exception, string caller, string message, params object[] args);

        void Audit(string eventId, string caller, string message, params object[] args);

        void Count(string eventId);
    }

    public interface ICrashReporter
    {
        void Crash(CrashLevel level, Exception exception, string caller, string message, object[] args);
    }

    public interface IAuditReporter
    {
        void Audit(string eventId, string caller, string message, params object[] args);
    }

    public interface IMetricReporter
    {
        void Count(string eventId);
    }

    public enum CrashLevel
    {
        NonCritical = 0,
        Critical = 1
    }
}