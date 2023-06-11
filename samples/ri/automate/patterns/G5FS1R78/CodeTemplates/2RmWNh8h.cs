using System.Reflection;
using Api.Common;
using Application.Storage.Interfaces;
using ApplicationServices.Interfaces;
using ApplicationServices.Interfaces.Eventing.Notifications;
using {{DomainName | string.pascalplural}}Application;
using {{DomainName | string.pascalplural}}Application.Storage;
using {{DomainName | string.pascalplural}}Domain;
using {{DomainName | string.pascalplural}}Storage;
using Common;
using Domain.Interfaces.Entities;
using Funq;
using InfrastructureServices.ApplicationServices;
using InfrastructureServices.Eventing.Notifications;
using ServiceStack;
using ServiceStack.Configuration;
using IRepository = Storage.IRepository;

namespace {{DomainName | string.pascalplural}}ApiHost
{
    public class ServiceHost : AppHostBase
    {
        private static readonly Assembly[] AssembliesContainingServicesAndValidators = {typeof(Startup).Assembly};
        public static readonly Assembly[] AssembliesContainingDomainEntities =
        {
            typeof(EntityEvent).Assembly,
            typeof({{DomainName | string.pascalsingular}}Entity).Assembly
        };

        public ServiceHost() : base("My{{DomainName | string.pascalplural}}Api", AssembliesContainingServicesAndValidators)
        {
        }

        public override void Configure(Container container)
        {
            var debugEnabled = AppSettings.Get(nameof(HostConfig.DebugMode), false);
            this.ConfigureServiceHost<ServiceHost>(debugEnabled);
            this.ConfigureRequestValidation(AssembliesContainingServicesAndValidators);
            this.ConfigureRepositories(AssembliesContainingDomainEntities);
            this.ConfigureEventing<{{DomainName | string.pascalsingular}}Entity>(c => new[]
                {
                    new {{DomainName | string.pascalsingular}}EntityReadModelProjection(c.Resolve<IRecorder>(), c.Resolve<IRepository>())
                }, c => DomainEventPublisherSubscriberPair.None
            );

            RegisterDependencies(container);
        }

        private static void RegisterDependencies(Container container)
        {
            container.AddSingleton<IIdentifierFactory, {{DomainName | string.pascalsingular}}IdentifierFactory>();

            container.AddSingleton<I{{DomainName | string.pascalsingular}}Storage>(c =>
                new {{DomainName | string.pascalsingular}}Storage(c.Resolve<IRecorder>(), c.Resolve<IDomainFactory>(),
                    c.Resolve<IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity>>(), c.Resolve<IRepository>()));
            container.AddSingleton<I{{DomainName | string.pascalplural}}Application, {{DomainName | string.pascalplural}}Application.{{DomainName | string.pascalplural}}Application>();
        }
    }
}