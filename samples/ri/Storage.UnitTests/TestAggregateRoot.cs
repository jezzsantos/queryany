using System;
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
            Events = new List<EntityEvent>();
        }

        public List<EntityEvent> Events { get; set; }

        public bool ClearedChanges { get; private set; }

        public IEnumerable<EntityEvent> LoadedChanges { get; private set; }

        public Identifier Id { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public DateTime? LastPersistedAtUtc { get; }

        public int ChangeVersion { get; set; }

        public List<EntityEvent> GetChanges()
        {
            return Events;
        }

        public void ClearChanges()
        {
            ClearedChanges = true;
        }

        public void OnStateChanged(IChangeEvent @event)
        {
            throw new NotImplementedException();
        }

        public void LoadChanges(IEnumerable<EntityEvent> history)
        {
            LoadedChanges = history;
        }
    }
}