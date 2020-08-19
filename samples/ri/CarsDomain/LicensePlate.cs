﻿using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class LicensePlate : ValueObjectBase<LicensePlate>
    {
        public LicensePlate(string jurisdiction, string number)
        {
            jurisdiction.GuardAgainstNullOrEmpty(nameof(number));
            number.GuardAgainstNullOrEmpty(nameof(number));
            jurisdiction.GuardAgainstInvalid(Validations.Car.Jurisdiction, nameof(jurisdiction));
            number.GuardAgainstInvalid(Validations.Car.Number, nameof(number));
            Jurisdiction = jurisdiction;
            Number = number;
        }

        public string Jurisdiction { get; private set; }

        public string Number { get; private set; }

        public override void Rehydrate(string value)
        {
            var parts = RehydrateToList(value);
            Jurisdiction = parts[0];
            Number = parts[1];
        }

        public static ValueObjectFactory<LicensePlate> Instantiate()
        {
            return (property, container) =>
            {
                var parts = RehydrateToList(property);
                return new LicensePlate(parts[0], parts[1]);
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {Jurisdiction, Number};
        }
    }
}