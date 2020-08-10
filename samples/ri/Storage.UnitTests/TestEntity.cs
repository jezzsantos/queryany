﻿using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny;
using ServiceStack;

namespace Storage.UnitTests
{
    [EntityName("testentities")]
    public class TestEntity : IPersistableEntity
    {
        public string AStringValue { get; set; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool ABooleanValue { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public double ADoubleValue { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        internal string AnInternalProperty { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DateTime CreatedAtUtc { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DateTime LastModifiedAtUtc { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public Identifier Id { get; }

        public Dictionary<string, object> Dehydrate()
        {
            return this.ToObjectDictionary();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            this.PopulateWith(properties);
        }
    }
}