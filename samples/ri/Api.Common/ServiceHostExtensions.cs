using System;
using System.Collections.Generic;
using System.Net;
using Domain.Interfaces;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Validation;

namespace Api.Common
{
    public static class ServiceHostExtensions
    {
        public static void ConfigureServiceHost(this ServiceStackHost appHost, bool debugEnabled)
        {
            appHost.SetConfig(new HostConfig
            {
                DebugMode = debugEnabled,
                DefaultRedirectPath = "/metadata",
                MapExceptionToStatusCode = SetupExceptionToStatusCodeMap()
            });

            SetupJsonResponses();
        }

        private static Dictionary<Type, int> SetupExceptionToStatusCodeMap()
        {
            return new Dictionary<Type, int>
            {
                {typeof(ValidationError), (int) HttpStatusCode.BadRequest},
                {typeof(ArgumentException), (int) HttpStatusCode.BadRequest},
                {typeof(ArgumentNullException), (int) HttpStatusCode.BadRequest},
                {typeof(ArgumentOutOfRangeException), (int) HttpStatusCode.BadRequest},
                {typeof(InvalidOperationException), (int) HttpStatusCode.BadRequest},
                {typeof(RuleViolationException), (int) HttpStatusCode.BadRequest},
                {typeof(AuthenticationException), (int) HttpStatusCode.Unauthorized},
                {typeof(UnauthorizedAccessException), (int) HttpStatusCode.Unauthorized},
                {typeof(ForbiddenException), (int) HttpStatusCode.Forbidden},
                {typeof(ResourceNotFoundException), (int) HttpStatusCode.NotFound},
                {typeof(RoleViolationException), (int) HttpStatusCode.NotFound},
                {typeof(MethodNotAllowedException), (int) HttpStatusCode.MethodNotAllowed},
                {typeof(ResourceConflictException), (int) HttpStatusCode.Conflict}
            };
        }

        private static void SetupJsonResponses()
        {
            JsConfig.DateHandler = DateHandler.ISO8601;
            JsConfig.AssumeUtc = true;
            JsConfig.AlwaysUseUtc = true;
            JsConfig.ExcludeTypeInfo = true;

            JsConfig<DateTime>.SerializeFn = dt => !dt.HasValue()
                ? null
                : dt.ToIso8601();
            JsConfig<DateTime?>.SerializeFn = dt => !dt.HasValue || !dt.Value.HasValue()
                ? null
                : dt.Value.ToIso8601();
        }
    }
}