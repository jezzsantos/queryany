using System.Collections.Generic;
using CarsDomain.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class LicensePlate : ValueObjectBase<LicensePlate>
    {
        public static readonly List<string> Jurisdictions = new List<string> {"New Zealand", "Australia"};

        public LicensePlate(string jurisdiction, string number)
        {
            jurisdiction.GuardAgainstNullOrEmpty(nameof(number));
            number.GuardAgainstNullOrEmpty(nameof(number));
            jurisdiction.GuardAgainstInvalid(val => Jurisdictions.Contains(val), nameof(jurisdiction),
                Resources.LicensePlate_UnknownJurisdiction);
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
                var parts = RehydrateToList(property, false);
                return new LicensePlate(parts[0], parts[1]);
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {Jurisdiction, Number};
        }
    }
}