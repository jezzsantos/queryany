﻿using System.Collections.Generic;
using Services.Interfaces.Entities;

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