using System;
using System.Reflection;
using Api.Common;
using Api.Common.Validators;
using ApplicationServices;
using CarsApplication;
using CarsApplication.Storage;
using CarsDomain;
using CarsStorage;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Funq;
using InfrastructureServices.ApplicationServices;
using InfrastructureServices.Eventing.Notifications;
using InfrastructureServices.Eventing.ReadModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Validation;
using Storage;
using Storage.Azure;
using Storage.Interfaces;
using Storage.ReadModels;
using IRepository = Storage.IRepository;

namespace CarsApi
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndDependencies = {typeof(Startup).Assembly};
        public static readonly Assembly[] AssembliesContainingDomainEntities =
        {
            typeof(EntityEvent).Assembly,
            typeof(CarEntity).Assembly
        };
        private static IRepository repository;
        private IReadModelProjectionSubscription readModelProjectionSubscription;

        public ServiceHost() : base("MyCarsApi", AssembliesContainingServicesAndDependencies)
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
            container.AddSingleton<IIdentifierFactory, CarIdentifierFactory>();
            container.AddSingleton<IChangeEventMigrator>(c => new ChangeEventTypeMigrator());
            container.AddSingleton<IDomainFactory>(c => DomainFactory.CreateRegistered(
                c.Resolve<IDependencyContainer>(), AssembliesContainingDomainEntities));
            container.AddSingleton<IEventStreamStorage<CarEntity>>(c =>
                new GeneralEventStreamStorage<CarEntity>(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IChangeEventMigrator>(),
                    ResolveRepository(c)));
            container.AddSingleton<ICarStorage>(c =>
                new CarStorage(c.Resolve<ILogger>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IEventStreamStorage<CarEntity>>(), ResolveRepository(c)));
            container.AddSingleton<ICarsApplication, CarsApplication.CarsApplication>();
            container.AddSingleton<IPersonsService>(c =>
                new PersonsServiceClient(c.Resolve<IAppSettings>().GetString("PersonsApiBaseUrl")));
            container.AddSingleton<IReadModelProjectionSubscription>(c => new InProcessReadModelProjectionSubscription(
                c.Resolve<ILogger>(),
                new ReadModelProjector(c.Resolve<ILogger>(),
                    new ReadModelCheckpointStore(c.Resolve<ILogger>(), c.Resolve<IIdentifierFactory>(),
                        c.Resolve<IDomainFactory>(),
                        ResolveRepository(c)),
                    c.Resolve<IChangeEventMigrator>(),
                    new CarEntityReadModelProjection(c.Resolve<ILogger>(), ResolveRepository(c))),
                c.Resolve<IEventStreamStorage<CarEntity>>()));
            container.AddSingleton<IChangeEventNotificationSubscription>(c =>
                new InProcessChangeEventNotificationSubscription(
                    c.Resolve<ILogger>(),
                    new DomainEventNotificationProducer(c.Resolve<ILogger>(), c.Resolve<IChangeEventMigrator>(),
                        new DomainEventPublisherSubscriberPair(new PersonDomainEventPublisher(),
                            new CarManagerEventSubscriber(c.Resolve<ICarsApplication>()))),
                    c.Resolve<IEventStreamStorage<CarEntity>>()));
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