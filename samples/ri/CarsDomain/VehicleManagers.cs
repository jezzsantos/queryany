using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class VehicleManagers : ValueObjectBase<VehicleManagers>
    {
        private List<Identifier> managerIds;

        public VehicleManagers()
        {
            this.managerIds = new List<Identifier>();
        }

        public IReadOnlyList<Identifier> ManagerIds => this.managerIds;

        public void Add(Identifier managerId)
        {
            managerId.GuardAgainstNull(nameof(managerId));

            if (!this.managerIds.Contains(managerId))
            {
                this.managerIds.Add(managerId);
            }
        }

        public override string Dehydrate()
        {
            return this.managerIds
                .Select(man => man)
                .Join(";");
        }

        public override void Rehydrate(string value)
        {
            if (value.HasValue())
            {
                this.managerIds = value.SafeSplit(";")
                    .Select(Identifier.Create)
                    .ToList();
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {ManagerIds};
        }
    }
}