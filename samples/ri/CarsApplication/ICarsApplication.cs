using System;
using Application.Resources;
using Domain.Interfaces;

namespace CarsApplication
{
    public interface ICarsApplication
    {
        Car Create(ICurrentCaller caller, int year, string make, string model);

        SearchResults<Car> SearchAvailable(ICurrentCaller caller, DateTime fromUtc, DateTime toUtc,
            SearchOptions searchOptions, GetOptions getOptions);

        Car Offline(ICurrentCaller caller, string id, DateTime fromUtc, DateTime toUtc);

        Car Register(ICurrentCaller caller, string id, string jurisdiction, string number);

        void UpdateManagerEmail(ICurrentCaller caller, string managerId, string email);
    }
}