using Common;
using Domain.Interfaces.Entities;
using InfrastructureServices.Eventing.ReadModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonsApplication.ReadModels;
using PersonsApplication.Storage;
using PersonsDomain;
using PersonsStorage;
using ServiceStack;
using ServiceStack.Configuration;
using Storage;
using Storage.Interfaces;

namespace PersonsApi.IntegrationTests
{
    public class TestStartup : ModularStartup
    {
        public new void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var appSettings = new NetCoreAppSettings(Configuration);
            app.UseServiceStack(new ServiceHost
            {
                BeforeConfigure =
                {
                    host =>
                    {
                        var container = host.GetContainer();
                        container.AddSingleton<IAppSettings>(appSettings);
                        host.AppSettings = appSettings;
                    }
                },
                AfterConfigure =
                {
                    host =>
                    {
                        // Override services for testing
                        var container = host.GetContainer();

                        var repository = new InProcessInMemRepository();

                        container.AddSingleton<IQueryStorage<Person>>(new GeneralQueryStorage<Person>(
                            container.Resolve<IRecorder>(),
                            container.Resolve<IDomainFactory>(), repository));
                        container.AddSingleton<IEventStreamStorage<PersonEntity>>(
                            new GeneralEventStreamStorage<PersonEntity>(
                                container.Resolve<IRecorder>(),
                                container.Resolve<IDomainFactory>(),
                                container.Resolve<IChangeEventMigrator>(), repository));
                        container.AddSingleton<IPersonStorage>(c =>
                            new PersonStorage(c.Resolve<IEventStreamStorage<PersonEntity>>(),
                                c.Resolve<IQueryStorage<Person>>()));

                        container.AddSingleton<IReadModelProjectionSubscription>(c =>
                            new InProcessReadModelProjectionSubscription(
                                c.Resolve<IRecorder>(), c.Resolve<IIdentifierFactory>(),
                                c.Resolve<IChangeEventMigrator>(),
                                c.Resolve<IDomainFactory>(), repository,
                                new[]
                                {
                                    new PersonEntityReadModelProjection(c.Resolve<IRecorder>(), repository)
                                },
                                c.Resolve<IEventStreamStorage<PersonEntity>>()));
                    }
                }
            });
        }
    }
}