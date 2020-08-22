using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class VehicleOwner : SingleValueObjectBase<VehicleOwner, string>
    {
        public VehicleOwner(string ownerId) : base(ownerId)
        {
            ownerId.GuardAgainstNullOrEmpty(nameof(ownerId));
        }

        public static ValueObjectFactory<VehicleOwner> Instantiate()
        {
            return (property, container) => new VehicleOwner(property);
        }

        protected override string ToValue(string value)
        {
            return value;
        }
    }
}