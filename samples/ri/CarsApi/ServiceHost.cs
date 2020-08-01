using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using CarsApi.Validators;
using CarsDomain;
using CarsDomain.Entities;
using CarsStorage;
using Funq;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Text;
using ServiceStack.Validation;
using Storage;
using Storage.Interfaces;

namespace CarsApi
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies = {typeof(Startup).Assembly};

        public ServiceHost() : base("MyCarsApi", AssembliesContainingServicesAndDependencies)
        {
        }

        public override void Configure(Container container)
        {
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            SetupServiceConfig(debugEnabled);

            RegisterValidators(container);
            RegisterDependencies(container);
        }

        private void SetupServiceConfig(bool debugEnabled)
        {
            SetConfig(new HostConfig
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
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IStorage<CarEntity>>(c =>
                CarEntityAzureStorage.Create(c.Resolve<ILogger>(), container.Resolve<IAppSettings>(),
                    new GuidIdentifierFactory()));
            container.AddSingleton<ICars, Cars>();
        }

        private void RegisterValidators(Container container)
        {
            Plugins.Add(new ValidationFeature());
            container.RegisterValidators(AssembliesContainingServicesAndDependencies);
            container.AddSingleton<IHasSearchOptionsValidator, HasSearchOptionsValidator>();
            container.AddSingleton<IHasGetOptionsValidator, HasGetOptionsValidator>();
        }
    }
}