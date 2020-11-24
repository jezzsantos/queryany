using ApplicationServices;
using CarsApplication;
using Domain.Interfaces.Entities;
using InfrastructureServices.Auth;
using PersonsDomain;
using QueryAny.Primitives;

namespace InfrastructureServices
{
    public class GeneralDomainEventSubscriber : IDomainEventSubscriber
    {
        private readonly ICarsApplication carsApplication;

        public GeneralDomainEventSubscriber(ICarsApplication carsApplication)
        {
            carsApplication.GuardAgainstNull(nameof(carsApplication));
            this.carsApplication = carsApplication;
        }

        public bool Notify(IChangeEvent originalEvent)
        {
            switch (originalEvent)
            {
                case Events.Person.EmailChanged e:
                    this.carsApplication.UpdateManagerEmail(new AnonymousCaller(), e.EntityId,
                        e.EmailAddress);
                    return true;

                default:
                    return false;
            }
        }
    }
}