using System.Collections.Generic;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;

namespace CarsApplication.Storage
{
    public interface ICarStorage
    {
        CarEntity Create(CarEntity car);

        CarEntity Get(Identifier toIdentifier);

        CarEntity Update(CarEntity car);

        List<CarEntity> SearchAvailable(SearchOptions options);
    }
}