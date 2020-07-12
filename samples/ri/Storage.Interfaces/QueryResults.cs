﻿using System.Collections.Generic;

namespace Storage.Interfaces
{
    public class QueryResults<TEntity> where TEntity : IIdentifiableEntity, new()
    {
        public QueryResults(List<TEntity> results)
        {
            Results = results;
        }

        public List<TEntity> Results { get; }
    }
}