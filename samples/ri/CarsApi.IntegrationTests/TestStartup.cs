using ApplicationServices;
using CarsApplication.ReadModels;
using CarsApplication.Storage;
using CarsDomain;
using CarsStorage;
using Domain.Interfaces.Entities;
using InfrastructureServices.Eventing.ReadModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Configuration;
using Storage;
using Storage.Interfaces;

namespace CarsApi.IntegrationTests
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
                    }
                },
                AfterConfigure =
                {
                    host =>
                    {
                        // Override services for testing
                        var container = host.GetContainer();
                        container.AddSingleton<IPersonsService, StubPersonsService>();

                        var repository = new InProcessInMemRepository();

                        container.AddSingleton<IQueryStorage<Car>>(new GeneralQueryStorage<Car>(
                            container.Resolve<ILogger>(),
                            container.Resolve<IDomainFactory>(), repository));
                        container.AddSingleton<IQueryStorage<Unavailability>>(new GeneralQueryStorage<Unavailability>(
                            container.Resolve<ILogger>(),
                            container.Resolve<IDomainFactory>(), repository));
                        container.AddSingleton<IEventStreamStorage<CarEntity>>(
                            new GeneralEventStreamStorage<CarEntity>(
                                container.Resolve<ILogger>(),
                                container.Resolve<IDomainFactory>(),
                                container.Resolve<IChangeEventMigrator>(), repository));
                        container.AddSingleton<ICarStorage>(c =>
                            new CarStorage(c.Resolve<IEventStreamStorage<CarEntity>>(),
                                c.Resolve<IQueryStorage<Car>>(), c.Resolve<IQueryStorage<Unavailability>>()));

                        container.AddSingleton<IReadModelProjectionSubscription>(c =>
                            new InProcessReadModelProjectionSubscription(
                                c.Resolve<ILogger>(), c.Resolve<IIdentifierFactory>(),
                                c.Resolve<IChangeEventMigrator>(),
                                c.Resolve<IDomainFactory>(), repository,
                                new[]
                                {
                                    new CarEntityReadModelProjection(c.Resolve<ILogger>(), repository)
                                },
                                c.Resolve<IEventStreamStorage<CarEntity>>()));
                    }
                }
            });
        }
    }
}