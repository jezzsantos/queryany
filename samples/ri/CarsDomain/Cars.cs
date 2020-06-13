using System;
using System.Collections.Generic;
using CarsDomain.Entities;
using QueryAny;
using Services.Interfaces;
using Services.Interfaces.Resources;
using ServiceStack;
using Storage.Interfaces;

namespace CarsDomain
{
    public class Cars : DomainObject
    {
        public IStorage<CarEntity> Storage { get; set; }

        public List<Car> SearchAvailable(SearchOptions searchOptions, GetOptions getOptions)
        {
            var query = Query.From<CarEntity>()
                .Where(e => e.OccupiedUntilUtc, ConditionOperator.LessThan, DateTime.UtcNow);
            var cars = Storage.Query(query, searchOptions);

            // TODO: Do what you have to do any expansions defined in GetOptions

            return cars.Results.ConvertAll(c => c.ConvertTo<Car>());
        }
    }
}