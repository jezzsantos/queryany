using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class VehicleOwner : ValueObjectBase<VehicleOwner>
    {
        public VehicleOwner(CarOwner owner)
        {
            owner.GuardAgainstNull(nameof(owner));
            Owner = owner.Id.ToIdentifier();
        }

        public Identifier Owner { get; private set; }

        public override void Rehydrate(string value)
        {
            Owner = value?.ToIdentifier();
        }

        public static ValueObjectFactory<VehicleOwner> Rehydrate()
        {
            return (property, container) => new VehicleOwner(new CarOwner {Id = property});
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {Owner};
        }
    }
}