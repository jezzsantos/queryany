using System.Collections.Generic;
using QueryAny;

namespace Storage.Interfaces
{
    public class QueryResults<TEntity> where TEntity : IQueryableEntity
    {
        public QueryResults(List<TEntity> results)
        {
            Results = results;
        }

        public List<TEntity> Results { get; }
    }
}