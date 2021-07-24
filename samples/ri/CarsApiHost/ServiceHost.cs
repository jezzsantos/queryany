using System.Reflection;
using Api.Common;
using Application.Storage.Interfaces;
using ApplicationServices.Interfaces;
using ApplicationServices.Interfaces.Eventing.Notifications;
using CarsApplication;
using CarsApplication.Storage;
using CarsDomain;
using CarsStorage;
using Common;
using Domain.Interfaces.Entities;
using Funq;
using InfrastructureServices.ApplicationServices;
using InfrastructureServices.Eventing.Notifications;
using ServiceStack;
using ServiceStack.Configuration;
using IRepository = Storage.IRepository;

namespace CarsApiHost
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndValidators = {typeof(Startup).Assembly};
        public static readonly Assembly[] AssembliesContainingDomainEntities =
        {
            typeof(EntityEvent).Assembly,
            typeof(CarEntity).Assembly
        };

        public ServiceHost() : base("MyCarsApi", AssembliesContainingServicesAndValidators)
        {
        }

        public override void Configure(Container container)
        {
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            this.ConfigureServiceHost<ServiceHost>(debugEnabled);
            this.ConfigureRequestValidation(AssembliesContainingServicesAndValidators);
            this.ConfigureRepositories(AssembliesContainingDomainEntities);
            this.ConfigureEventing<CarEntity>(c => new[]
                {
                    new CarEntityReadModelProjection(c.Resolve<IRecorder>(), c.Resolve<IRepository>())
                }, c => new[]
                {
                    new DomainEventPublisherSubscriberPair(new PersonDomainEventPublisher(),
                        new CarManagerEventSubscriber(c.Resolve<ICarsApplication>()))
                }
            );

            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IIdentifierFactory, CarIdentifierFactory>();

            container.AddSingleton<ICarStorage>(c =>
                new CarStorage(c.Resolve<IRecorder>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IEventStreamStorage<CarEntity>>(), c.Resolve<IRepository>()));
            container.AddSingleton<ICarsApplication, CarsApplication.CarsApplication>();
            container.AddSingleton<IPersonsService>(c =>
                new PersonsServiceClient(c.Resolve<IAppSettings>().GetString("ApplicationServices:PersonsApiBaseUrl")));
        }
    }
}