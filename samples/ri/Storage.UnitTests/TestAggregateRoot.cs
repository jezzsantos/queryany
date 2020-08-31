﻿using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny;

namespace Storage.UnitTests
{
    [EntityName("acontainername")]
    public class TestAggregateRoot : IPersistableAggregateRoot
    {
        public TestAggregateRoot(Identifier identifier)
        {
            Id = identifier;
            Events = new List<EventEntity>();
        }

        public List<EventEntity> Events { get; set; }

        public bool ClearedChanges { get; private set; }

        public IEnumerable<EventEntity> LoadedChanges { get; private set; }

        public Identifier Id { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DateTime? LastPersistedAtUtc { get; }

        public Dictionary<string, object> Dehydrate()
        {
            throw new NotImplementedException();
        }

        public void Rehydrate(IReadOnlyDictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        public int ChangeVersion { get; set; }

        public List<EventEntity> GetChanges()
        {
            return Events;
        }

        public void ClearChanges()
        {
            ClearedChanges = true;
        }

        public void OnStateChanged(object @event)
        {
            throw new NotImplementedException();
        }

        public void LoadChanges(IEnumerable<EventEntity> history)
        {
            LoadedChanges = history;
        }
    }
}