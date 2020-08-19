using Domain.Interfaces.Entities;
using Domain.Interfaces.Resources;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class VehicleOwner : SingleValueObjectBase<VehicleOwner, string>
    {
        public VehicleOwner(CarOwner owner) : base(owner.Id)
        {
            owner.GuardAgainstNull(nameof(owner));
        }

        public static ValueObjectFactory<VehicleOwner> Instantiate()
        {
            return (property, container) => new VehicleOwner(new CarOwner {Id = property});
        }

        protected override string ToValue(string value)
        {
            return value;
        }
    }
}