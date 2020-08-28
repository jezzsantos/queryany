﻿using System;
using System.Collections.Generic;
using CarsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;

namespace CarsApplication.Storage
{
    public interface ICarStorage
    {
        CarEntity Load(Identifier id);

        CarEntity Save(CarEntity car);

        List<CarEntity> SearchAvailable(DateTime fromUtc, DateTime toUtc, SearchOptions options);
    }
}