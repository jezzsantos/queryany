﻿using System;
using System.Collections.Generic;
using CarsDomain.Properties;
using Common;
using Domain.Interfaces.Entities;
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
            year.GuardAgainstInvalid(y => y >= MinYear && y <= MaxYear, nameof(year),
                Resources.Manufacturer_InvalidYear.Format(MinYear, MaxYear));
            make.GuardAgainstInvalid(m => Makes.Contains(m), nameof(make), Resources.Manufacturer_UnknownMake);
            model.GuardAgainstInvalid(m => Models.Contains(m), nameof(model), Resources.Manufacturer_UnknownModel);

            Year = year;
            Make = make;
            Model = model;
        }

        public int Year { get; }

        public string Make { get; }

        public string Model { get; }

        public static ValueObjectFactory<Manufacturer> Rehydrate()
        {
            return (value, container) =>
            {
                var parts = RehydrateToList(value, false);
                return new Manufacturer(parts[0].ToInt(0), parts[1], parts[2]);
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Year, Make, Model};
        }
    }
}