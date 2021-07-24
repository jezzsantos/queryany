using System.Reflection;
using Api.Common;
using Application.Storage.Interfaces;
using ApplicationServices.Interfaces.Eventing.Notifications;
using Common;
using Domain.Interfaces.Entities;
using DomainServices.Interfaces;
using Funq;
using PersonsApplication;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using IRepository = Storage.IRepository;

namespace PersonsApiHost
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndValidators = {typeof(Startup).Assembly};
        public static readonly Assembly[] AssembliesContainingDomainEntities =
        {
            typeof(EntityEvent).Assembly,
            typeof(PersonEntity).Assembly
        };

        public ServiceHost() : base("Persons", AssembliesContainingServicesAndValidators)
        {
        }

        public override void Configure(Container container)
        {
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            this.ConfigureServiceHost<ServiceHost>(debugEnabled);
            this.ConfigureRequestValidation(AssembliesContainingServicesAndValidators);
            this.ConfigureRepositories(AssembliesContainingDomainEntities);
            this.ConfigureEventing<PersonEntity>(c => new[]
            {
                new PersonEntityReadModelProjection(c.Resolve<IRecorder>(), c.Resolve<IRepository>())
            }, c => DomainEventPublisherSubscriberPair.None);

            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IIdentifierFactory, PersonIdentifierFactory>();

            container.AddSingleton<IPersonStorage>(c =>
                new PersonStorage(c.Resolve<IRecorder>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IEventStreamStorage<PersonEntity>>(),
                    c.Resolve<IRepository>()));
            container.AddSingleton<IPersonsApplication, PersonsApplication.PersonsApplication>();
            container.AddSingleton<IEmailService, EmailService>();
        }
    }
}