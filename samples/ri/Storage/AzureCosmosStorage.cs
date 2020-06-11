using System;
using QueryAny;
using Services.Interfaces;
using Storage.Interfaces;

namespace Storage
{
    public class AzureCosmosStorage<TEntity> : IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        public void Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public TEntity Update(TEntity entity, bool ignoreConcurrency)
        {
            throw new NotImplementedException();
        }

        public void Delete(string id, bool ignoreConcurrency)
        {
            throw new NotImplementedException();
        }

        public TEntity Get(string id)
        {
            throw new NotImplementedException();
        }

        public QueryResults<TEntity> Query(WhereClause<TEntity> query, SearchOptions options)
        {
            //TODO: convert the query to string


            throw new NotImplementedException();
        }

        public long Count()
        {
            throw new NotImplementedException();
        }
    }
}