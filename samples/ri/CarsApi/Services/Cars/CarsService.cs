using System;
using Api.Common;
using Api.Interfaces;
using Api.Interfaces.ServiceOperations;
using CarsApplication;
using Domain.Interfaces.Resources;
using QueryAny.Primitives;
using ServiceStack;

namespace CarsApi.Services.Cars
{
    internal class CarsService : Service
    {
        private readonly ICarsApplication carsApplication;

        public CarsService(ICarsApplication carsApplication)
        {
            carsApplication.GuardAgainstNull(nameof(carsApplication));

            this.carsApplication = carsApplication;
        }

        public SearchAvailableCarsResponse Get(SearchAvailableCarsRequest request)
        {
            var fromUtc = request.FromUtc.GetValueOrDefault(DateTime.MinValue);
            var toUtc = request.ToUtc.GetValueOrDefault(DateTime.MaxValue);
            var available = this.carsApplication.SearchAvailable(Request.ToCaller(), fromUtc, toUtc,
                request.ToSearchOptions(defaultSort: Reflector<Car>.GetPropertyName(c => c.Id)),
                request.ToGetOptions());
            return new SearchAvailableCarsResponse
            {
                Cars = available.Results,
                Metadata = available.Metadata
            };
        }

        public CreateCarResponse Post(CreateCarRequest request)
        {
            return new CreateCarResponse
            {
                Car = this.carsApplication.Create(Request.ToCaller(), request.Year, request.Make, request.Model)
            };
        }

        public RegisterCarResponse Put(RegisterCarRequest request)
        {
            return new RegisterCarResponse
            {
                Car = this.carsApplication.Register(Request.ToCaller(), request.Id, request.Jurisdiction,
                    request.Number)
            };
        }

        public OfflineCarResponse Put(OfflineCarRequest request)
        {
            return new OfflineCarResponse
            {
                Car = this.carsApplication.Offline(Request.ToCaller(), request.Id, request.FromUtc, request.ToUtc)
            };
        }
    }
}