﻿using System.Collections.Generic;
using System.Linq;
using Common;
using Domain.Interfaces.Entities;

namespace CarsDomain
{
    public class VehicleManagers : ValueObjectBase<VehicleManagers>
    {
        private readonly List<Identifier> managers;

        public VehicleManagers()
        {
            this.managers = new List<Identifier>();
        }

        public IReadOnlyList<Identifier> Managers => this.managers;

        public void Add(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            if (!this.managers.Contains(id))
            {
                this.managers.Add(id);
            }
        }

        public override string Dehydrate()
        {
            return this.managers
                .Select(man => man)
                .Join(";");
        }

        public static ValueObjectFactory<VehicleManagers> Rehydrate()
        {
            return (value, container) => new VehicleManagers();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new[] {Managers};
        }
    }
}