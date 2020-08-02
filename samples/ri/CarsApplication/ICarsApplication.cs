﻿using System;
using Services.Interfaces;
using Services.Interfaces.Resources;

namespace CarsApplication
{
    public interface ICarsApplication
    {
        Car Create(ICurrentCaller caller, int year, string make, string model);

        SearchResults<Car> SearchAvailable(ICurrentCaller caller, SearchOptions searchOptions, GetOptions getOptions);

        Car Occupy(ICurrentCaller caller, string id, in DateTime untilUtc);
    }
}