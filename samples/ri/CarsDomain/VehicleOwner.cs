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

        public static ValueObjectFactory<VehicleOwner> Rehydrate()
        {
            return (property, container) => new VehicleOwner(property);
        }
    }
}