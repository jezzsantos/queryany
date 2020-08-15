using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class LicensePlate : ValueObjectBase<LicensePlate>
    {
        private string jurisdiction;
        private string number;

        public LicensePlate(string jurisdiction, string number)
        {
            jurisdiction.GuardAgainstNullOrEmpty(nameof(number));
            number.GuardAgainstNullOrEmpty(nameof(number));
            jurisdiction.GuardAgainstInvalid(Validations.Car.Jurisdiction, nameof(jurisdiction));
            number.GuardAgainstInvalid(Validations.Car.Number, nameof(number));
            this.jurisdiction = jurisdiction;
            this.number = number;
        }

        public override void Rehydrate(string value)
        {
            var parts = RehydrateToList(value);
            this.jurisdiction = parts[0];
            this.number = parts[1];
        }

        public static ValueObjectFactory<LicensePlate> Rehydrate()
        {
            return (property, container) =>
            {
                var parts = RehydrateToList(property);
                return new LicensePlate(parts[0], parts[1]);
            };
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {this.jurisdiction, this.number};
        }
    }
}