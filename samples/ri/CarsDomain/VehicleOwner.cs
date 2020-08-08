﻿using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace CarsDomain
{
    public class VehicleOwner : ValueType<VehicleOwner>
    {
        public VehicleOwner(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            Id = id;
        }

        public Identifier Id { get; private set; }

        public override string Dehydrate()
        {
            return $"{Id.Get()}";
        }

        public override void Rehydrate(string value)
        {
            if (value.HasValue())
            {
                Id = Identifier.Create(value);
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Id};
        }
    }
}