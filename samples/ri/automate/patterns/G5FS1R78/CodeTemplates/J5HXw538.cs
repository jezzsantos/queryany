using Application.Storage.Interfaces;
using ApplicationServices.Interfaces;
using {{DomainName | string.pascalplural}}ApiHost;
using {{DomainName | string.pascalplural}}Application.ReadModels;
using {{DomainName | string.pascalplural}}Application.Storage;
using {{DomainName | string.pascalplural}}Domain;
using {{DomainName | string.pascalplural}}Storage;
using Common;
using Domain.Interfaces.Entities;
using InfrastructureServices.Eventing.ReadModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceStack;
using ServiceStack.Configuration;
using Storage;

namespace {{DomainName | string.pascalplural}}Api.IntegrationTests
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

                        container.AddSingleton<IQueryStorage<{{DomainName | string.pascalsingular}}>>(new GeneralQueryStorage<{{DomainName | string.pascalsingular}}>(
                            container.Resolve<IRecorder>(),
                            container.Resolve<IDomainFactory>(), repository));
                        container.AddSingleton<IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity>>(
                            new GeneralEventStreamStorage<{{DomainName | string.pascalsingular}}Entity>(
                                container.Resolve<IRecorder>(),
                                container.Resolve<IDomainFactory>(),
                                container.Resolve<IChangeEventMigrator>(), repository));
                        container.AddSingleton<I{{DomainName | string.pascalsingular}}Storage>(c =>
                            new {{DomainName | string.pascalsingular}}Storage(c.Resolve<IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity>>(),
                                c.Resolve<IQueryStorage<{{DomainName | string.pascalsingular}}>>()));

                        container.AddSingleton<IReadModelProjectionSubscription>(c =>
                            new InProcessReadModelProjectionSubscription(
                                c.Resolve<IRecorder>(), c.Resolve<IIdentifierFactory>(),
                                c.Resolve<IChangeEventMigrator>(),
                                c.Resolve<IDomainFactory>(), repository,
                                new[]
                                {
                                    new {{DomainName | string.pascalsingular}}EntityReadModelProjection(c.Resolve<IRecorder>(), repository)
                                },
                                c.Resolve<IEventStreamStorage<{{DomainName | string.pascalsingular}}Entity>>()));
                    }
                }
            });
        }
    }
}