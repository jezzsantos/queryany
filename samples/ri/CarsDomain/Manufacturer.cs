using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;
using ServiceStack;

namespace CarsDomain
{
    public class Manufacturer : ValueObjectBase<Manufacturer>
    {
        public const int MinYear = 1917;
        public static readonly List<string> Makes = new List<string> {"Honda", "Toyota"};
        public static readonly int MaxYear = DateTime.UtcNow.AddYears(3).Year;
        public static readonly List<string> Models = new List<string> {"Civic", "Surf"};

        public Manufacturer(int year, string make, string model)
        {
            if (year < MinYear || year > MaxYear)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }
            if (!Makes.Contains(make))
            {
                throw new ArgumentOutOfRangeException(nameof(make));
            }
            if (!Models.Contains(model))
            {
                throw new ArgumentOutOfRangeException(nameof(model));
            }

            Year = year;
            Make = make;
            Model = model;
        }

        public int Year { get; private set; }

        public string Make { get; private set; }

        public string Model { get; private set; }

        public override void Rehydrate(string value)
        {
            if (value.HasValue())
            {
                var parts = RehydrateToList(value);
                Year = parts[0].ToInt(0);
                Make = parts[1];
                Model = parts[2];
            }
        }

        public static ValueObjectFactory<Manufacturer> Rehydrate()
        {
            return (value, container) =>
            {
                var parts = RehydrateToList(value);
                return new Manufacturer(parts[0].ToInt(0), parts[1], parts[2]);
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Year, Make, Model};
        }
    }
}