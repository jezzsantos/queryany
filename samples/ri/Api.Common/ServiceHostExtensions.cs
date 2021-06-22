﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Api.Common.Validators;
using Application.Interfaces;
using Application.Storage.Interfaces;
using Application.Storage.Interfaces.ReadModels;
using ApplicationServices.Interfaces;
using Common;
using Domain.Interfaces.Entities;
using Funq;
using InfrastructureServices.Eventing.Notifications;
using InfrastructureServices.Eventing.ReadModels;
using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Validation;
using Storage;
using IRepository = Storage.IRepository;

namespace Api.Common
{
    public static class ServiceHostExtensions
    {
        private static IChangeEventNotificationSubscription changeEventNotificationSubscription;
        private static IReadModelProjectionSubscription readModelProjectionSubscription;

        public static void ConfigureServiceHost<TServiceHost>(this ServiceStackHost appHost, bool debugEnabled)
            where TServiceHost : ServiceStackHost
        {
            appHost.SetConfig(new HostConfig
            {
                DebugMode = debugEnabled,
                DefaultRedirectPath = "/metadata",
                MapExceptionToStatusCode = SetupExceptionToStatusCodeMap()
            });

            appHost.Container.AddSingleton<IRecorder>(new Recorder<TServiceHost>());
            SetupExceptionShielding(appHost);
            SetupJsonResponses();
            appHost.RegisterService<HealthCheckService>();
        }

        public static void ConfigureRequestValidation(this IAppHost appHost,
            Assembly[] assembliesContainingServicesAndDependencies)
        {
            var container = appHost.GetContainer();
            appHost.Plugins.AddIfNotExists(new ValidationFeature());
            container.RegisterValidators(assembliesContainingServicesAndDependencies);
            container.AddSingleton<IHasSearchOptionsValidator, HasSearchOptionsValidator>();
            container.AddSingleton<IHasGetOptionsValidator, HasGetOptionsValidator>();
        }

        public static void ConfigureRepository(this IAppHost appHost,
            Assembly[] assembliesContainingDomainTypes)
        {
            var container = appHost.GetContainer();
            container.AddSingleton<IDependencyContainer>(new FunqDependencyContainer(container));
            if (!container.Exists<IDomainFactory>())
            {
                container.AddSingleton<IDomainFactory>(c => DomainFactory.CreateRegistered(
                    c.Resolve<IDependencyContainer>(), assembliesContainingDomainTypes));
            }
            else
            {
                container.Resolve<IDomainFactory>()
                    .RegisterDomainTypesFromAssemblies(assembliesContainingDomainTypes);
            }
            container.AddSingleton<IChangeEventMigrator>(c => new ChangeEventTypeMigrator());

            if (!container.Exists<ServiceStack.IRepository>())
            {
                container.AddSingleton(GetRepository(appHost));
            }
        }

        public static void ConfigureBlobository(this IAppHost appHost)
        {
            var container = appHost.GetContainer();

            if (!container.Exists<IBlobository>())
            {
                container.AddSingleton(GetBlobository(appHost));
            }
        }

        public static void ConfigureEventing<TAggregateRoot>(this IAppHost appHost,
            Func<Container, IEnumerable<IReadModelProjection>> projections,
            Func<Container, IEnumerable<IDomainEventPublisherSubscriberPair>> subscribers)
            where TAggregateRoot : IPersistableAggregateRoot
        {
            var container = appHost.GetContainer();
            container.AddSingleton<IEventStreamStorage<TAggregateRoot>>(c =>
                new GeneralEventStreamStorage<TAggregateRoot>(c.Resolve<IRecorder>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IChangeEventMigrator>(),
                    c.Resolve<IRepository>()));
            container.AddSingleton<IReadModelProjectionSubscription>(c => new InProcessReadModelProjectionSubscription(
                c.Resolve<IRecorder>(), c.Resolve<IIdentifierFactory>(), c.Resolve<IChangeEventMigrator>(),
                c.Resolve<IDomainFactory>(), c.Resolve<IRepository>(),
                projections(c), c.Resolve<IEventStreamStorage<TAggregateRoot>>()));

            container.AddSingleton<IChangeEventNotificationSubscription>(c =>
                new InProcessChangeEventNotificationSubscription(
                    c.Resolve<IRecorder>(), c.Resolve<IChangeEventMigrator>(),
                    subscribers(c),
                    c.Resolve<IEventStreamStorage<TAggregateRoot>>()));

            appHost.AfterInitCallbacks.Add(host =>
            {
                readModelProjectionSubscription = container.Resolve<IReadModelProjectionSubscription>();
                readModelProjectionSubscription.Start();
                changeEventNotificationSubscription = container.Resolve<IChangeEventNotificationSubscription>();
                changeEventNotificationSubscription.Start();
            });

            appHost.OnDisposeCallbacks.Add(host =>
            {
                (readModelProjectionSubscription as IDisposable)?.Dispose();
                (changeEventNotificationSubscription as IDisposable)?.Dispose();
            });
        }

        private static IRepository GetRepository(IAppHost appHost)
        {
            return LocalMachineFileRepository.FromSettings(appHost.AppSettings);
        }

        private static IBlobository GetBlobository(IAppHost appHost)
        {
            return LocalMachineFileRepository.FromSettings(appHost.AppSettings);
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

        private static void SetupExceptionShielding(IAppHost appHost)
        {
            appHost.ServiceExceptionHandlers.Add((httpReq, request, exception) =>
            {
                if (IsUnexpectedException(exception))
                {
                    var error = DtoUtils.CreateResponseStatus(exception, request, true);
                    var recorder = appHost.Resolve<IRecorder>();
                    var callerId = appHost.TryResolve<ICurrentCaller>()?.Id;
                    recorder.Crash(CrashLevel.NonCritical, exception, callerId, error.StackTrace);

                    return DtoUtils.CreateErrorResponse(request, WrapInUnhandledException(exception));
                }

                return null;
            });

            appHost.UncaughtExceptionHandlers.Add((request, response, operationName, exception) =>
            {
                var recorder = appHost.Resolve<IRecorder>();
                var callerId = appHost.TryResolve<ICurrentCaller>()?.Id;
                recorder.Crash(CrashLevel.Critical, WrapInUnhandledException(exception), callerId);
                response.EndRequest(true);
            });

            static Exception WrapInUnhandledException(Exception exception)
            {
                return new Exception(string.Empty, new UnexpectedException(exception));
            }
        }

        private static bool IsUnexpectedException(Exception ex)
        {
            var statusCode = ex.ToStatusCode();
            return statusCode >= (int) HttpStatusCode.InternalServerError;
        }
    }
}