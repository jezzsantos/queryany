using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class VehicleOwner : ValueTypeBase<VehicleOwner>
    {
        public VehicleOwner(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            Id = id;
        }

        public Identifier Id { get; private set; }

        public override string Dehydrate()
        {
            return $"{Id}";
        }

        public override void Rehydrate(string value)
        {
            if (value.HasValue())
            {
                Id = value.ToIdentifier();
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Id};
        }
    }
}