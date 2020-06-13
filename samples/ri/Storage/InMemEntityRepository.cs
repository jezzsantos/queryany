using System.Collections.Generic;
using System.Linq;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class InMemEntityRepository
    {
        private readonly IIdentifierFactory idFactory;

        private readonly Dictionary<string, Dictionary<string, IKeyedEntity>> store =
            new Dictionary<string, Dictionary<string, IKeyedEntity>>();

        public InMemEntityRepository(IIdentifierFactory idFactory)
        {
            Guard.AgainstNull(() => idFactory, idFactory);
            this.idFactory = idFactory;
        }

        public string Add(string entityName, IKeyedEntity entity)
        {
            if (!this.store.ContainsKey(entityName))
            {
                this.store.Add(entityName, new Dictionary<string, IKeyedEntity>());
            }

            var id = this.idFactory.Create(entity);
            entity.Id = id;
            this.store[entityName].Add(entity.Id, entity);
            return id;
        }

        public void Remove(string entityName, string id)
        {
            if (this.store.ContainsKey(entityName))
            {
                if (this.store[entityName].ContainsKey(id))
                {
                    this.store[entityName].Remove(id);
                }
            }
        }

        public void Update(string entityName, string id, IKeyedEntity entity)
        {
            if (this.store.ContainsKey(entityName))
            {
                if (this.store[entityName].ContainsKey(id))
                {
                    this.store[entityName][id] = entity;
                }
            }
        }

        public IKeyedEntity Get(string entityName, string id)
        {
            if (this.store.ContainsKey(entityName))
            {
                if (this.store[entityName].ContainsKey(id))
                {
                    return this.store[entityName][id];
                }
            }

            return null;
        }

        public long Count(string entityName)
        {
            if (this.store.ContainsKey(entityName))
            {
                return this.store[entityName].Count;
            }

            return 0;
        }

        public IEnumerable<IKeyedEntity> GetAll(string entityName)
        {
            if (this.store.ContainsKey(entityName))
            {
                return this.store[entityName].Values;
            }

            return Enumerable.Empty<IKeyedEntity>();
        }
    }
}