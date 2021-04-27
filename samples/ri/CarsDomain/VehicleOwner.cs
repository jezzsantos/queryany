using Domain.Interfaces;
using Domain.Interfaces.Entities;

namespace CarsDomain
{
    public class VehicleOwner : SingleValueObjectBase<VehicleOwner, string>
    {
        public VehicleOwner(string ownerId) : base(ownerId)
        {
            ownerId.GuardAgainstNullOrEmpty(nameof(ownerId));
        }

        public string OwnerId => Value;

        protected override string ToValue(string value)
        {
            return value;
        }

        public static ValueObjectFactory<VehicleOwner> Instantiate()
        {
            return (property, container) => new VehicleOwner(property);
        }
    }
}