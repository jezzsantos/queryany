using System.Collections.Generic;
using Services.Interfaces;
using Services.Interfaces.Resources;

namespace CarsDomain
{
    public interface ICars
    {
        Car Create(int year, string make, string model);

        List<Car> SearchAvailable(SearchOptions searchOptions, GetOptions getOptions);
    }
}