﻿using ApplicationServices.Interfaces.Eventing.Notifications;
using CarsApplication;
using Common;
using Domain.Interfaces.Entities;
using InfrastructureServices.Identity;
using PersonsDomain;

namespace InfrastructureServices.Eventing.Notifications
{
    public class CarManagerEventSubscriber : IDomainEventSubscriber
    {
        private readonly ICarsApplication carsApplication;

        public CarManagerEventSubscriber(ICarsApplication carsApplication)
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