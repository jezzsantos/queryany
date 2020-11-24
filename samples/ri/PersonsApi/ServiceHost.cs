using System;
using System.Reflection;
using Api.Common;
using Api.Common.Validators;
using ApplicationServices;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using DomainServices;
using Funq;
using InfrastructureServices.Eventing.Notifications;
using InfrastructureServices.Eventing.ReadModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PersonsApplication;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Validation;
using Storage;
using Storage.Azure;
using Storage.Interfaces;
using Storage.ReadModels;
using IRepository = Storage.IRepository;

namespace PersonsApi
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies = {typeof(Startup).Assembly};
        public static readonly Assembly[] AssembliesContainingDomainEntities =
        {
            typeof(EntityEvent).Assembly,
            typeof(PersonEntity).Assembly
        };
        private static IRepository repository;
        private IReadModelProjectionSubscription readModelProjectionSubscription;

        public ServiceHost() : base("MyPersonsApi", AssembliesContainingServicesAndDependencies)
        {
        }

        public override void Configure(Container container)
        {
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            this.ConfigureServiceHost(debugEnabled);

            RegisterValidators(container);
            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            static IRepository ResolveRepository(Container c)
            {
                return repository ??=
                    AzureCosmosSqlApiRepository.FromAppSettings(c.Resolve<IAppSettings>(), "Production");
            }

            container.AddSingleton<ILogger>(c => new Logger<ServiceHost>(new NullLoggerFactory()));
            container.AddSingleton<IDependencyContainer>(new FuncDependencyContainer(container));
            container.AddSingleton<IIdentifierFactory, PersonIdentifierFactory>();
            container.AddSingleton<IChangeEventMigrator>(c => new ChangeEventTypeMigrator());
            container.AddSingleton<IDomainFactory>(c =>
                DomainFactory.CreateRegistered(c.Resolve<IDependencyContainer>(), AssembliesContainingDomainEntities));
            container.AddSingleton<IEventStreamStorage<PersonEntity>>(c =>
                new GeneralEventStreamStorage<PersonEntity>(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IChangeEventMigrator>(),
                    ResolveRepository(c)));
            container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(), c.Resolve<IChangeEventMigrator>(),
                    ResolveRepository(c)));
            container.AddSingleton<IPersonsApplication, PersonsApplication.PersonsApplication>();
            container.AddSingleton<IEmailService, EmailService>();
            container.AddSingleton<IReadModelProjectionSubscription>(c => new InProcessReadModelProjectionSubscription(
                c.Resolve<ILogger>(),
                new ReadModelProjector(c.Resolve<ILogger>(),
                    new ReadModelCheckpointStore(c.Resolve<ILogger>(), c.Resolve<IIdentifierFactory>(),
                        c.Resolve<IDomainFactory>(),
                        ResolveRepository(c)),
                    c.Resolve<IChangeEventMigrator>(),
                    new PersonEntityReadModelProjection(c.Resolve<ILogger>(), ResolveRepository(c))),
                c.Resolve<IEventStreamStorage<PersonEntity>>()));
            container.AddSingleton<IChangeEventNotificationSubscription>(c =>
                new InProcessChangeEventNotificationSubscription(
                    c.Resolve<ILogger>(),
                    new DomainEventNotificationProducer(c.Resolve<ILogger>(), c.Resolve<IChangeEventMigrator>(),
                        DomainEventPublisherSubscriberPair.None),
                    c.Resolve<IEventStreamStorage<PersonEntity>>()));
        }

        private void RegisterValidators(Container container)
        {
            Plugins.Add(new ValidationFeature());
            container.RegisterValidators(AssembliesContainingServicesAndDependencies);
            container.AddSingleton<IHasSearchOptionsValidator, HasSearchOptionsValidator>();
            container.AddSingleton<IHasGetOptionsValidator, HasGetOptionsValidator>();
        }

        public override void OnAfterInit()
        {
            base.OnAfterInit();

            this.readModelProjectionSubscription = Container.Resolve<IReadModelProjectionSubscription>();
            this.readModelProjectionSubscription.Start();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            (this.readModelProjectionSubscription as IDisposable)?.Dispose();
        }
    }
}