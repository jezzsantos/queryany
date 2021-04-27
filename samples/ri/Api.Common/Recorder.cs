using System;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ServiceStack;
#if !TESTINGONLY
using Microsoft.Extensions.Logging.Abstractions;

#endif

namespace Api.Common
{
    public class Recorder<TServiceHost> : IRecorder
        where TServiceHost : ServiceStackHost
    {
        private readonly IAuditReporter auditor;
        private readonly ICrashReporter crasher;
        private readonly ILogger logger;
        private readonly IMetricReporter measurer;

        public Recorder() : this(
#if TESTINGONLY
            new Logger<TServiceHost>(LoggerFactory.Create(builder => builder.AddConsole())),
#else
            new Logger<TServiceHost>(new NullLoggerFactory()),
#endif
            new NullCrashReporter(), new NullAuditReporter(), new NullMetricReporter())
        {
        }

        private Recorder(ILogger logger, ICrashReporter crasher, IAuditReporter auditor, IMetricReporter measurer)
        {
            logger.GuardAgainstNull(nameof(logger));
            crasher.GuardAgainstNull(nameof(crasher));
            auditor.GuardAgainstNull(nameof(auditor));
            measurer.GuardAgainstNull(nameof(measurer));
            this.logger = logger;
            this.crasher = crasher;
            this.auditor = auditor;
            this.measurer = measurer;
        }

        public void TraceDebug(string message, params object[] args)
        {
            this.logger.LogDebug(message, args);
        }

        public void TraceInformation(string message, params object[] args)
        {
            this.logger.LogInformation(message, args);
        }

        public void TraceWarning(Exception exception, string message, params object[] args)
        {
            this.logger.LogWarning(exception, message, args);
        }

        public void TraceWarning(string message, params object[] args)
        {
            this.logger.LogWarning(message, args);
        }

        public void TraceError(Exception exception, string message, params object[] args)
        {
            this.logger.LogError(exception, message, args);
        }

        public void TraceError(string message, params object[] args)
        {
            this.logger.LogError(message, args);
        }

        public void Crash(CrashLevel level, Exception exception, string caller)
        {
            Crash(level, exception, caller, exception.Message);
        }

        public void Crash(CrashLevel level, Exception exception, string caller, string message, params object[] args)
        {
            var logLevel = level == CrashLevel.Critical
                ? LogLevel.Critical
                : LogLevel.Error;
            this.logger.Log(logLevel, exception, $"Crash: {message}", args);

            var errorSourceId = exception.GetBaseException().TargetSite?.ToString();
            if (errorSourceId.Exists())
            {
                this.measurer.Count($"Exceptions: {errorSourceId}");
            }

            this.crasher.Crash(level, exception, caller, message, args);
        }

        public void Audit(string eventId, string caller, string message, params object[] args)
        {
            TraceInformation($"Audit: {eventId}, {message}", args);
            Count(eventId);
            this.auditor.Audit(eventId, caller, message, args);
        }

        public void Count(string eventId)
        {
            TraceInformation($"Count: {eventId}");
            this.measurer.Count(eventId);
        }
    }
}