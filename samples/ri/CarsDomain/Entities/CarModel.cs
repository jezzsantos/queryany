using System.Collections.Generic;
using QueryAny.Primitives;
using Services.Interfaces.Entities;

namespace CarsDomain.Entities
{
    public class CarModel : ValueType<CarModel>
    {
        public CarModel(int year, string make, string model)
        {
            Year = year;
            Make = make;
            Model = model;
        }

        public int Year { get; private set; }
        public string Make { get; private set; }
        public string Model { get; private set; }

        public override string Dehydrate()
        {
            return $"{Year}::{Make}::{Model}";
        }

        public override void Rehydrate(string value)
        {
            if (value.HasValue())
            {
                var parts = value.SafeSplit("::");
                Year = parts[0].HasValue()
                    ? int.Parse(parts[0])
                    : 0;
                Make = parts[1];
                Model = parts[2];
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Year, Make, Model};
        }
    }
}