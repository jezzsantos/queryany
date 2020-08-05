﻿using System.Collections.Generic;
using Domain.Interfaces.Entities;

namespace Storage.Interfaces
{
    public class QueryResults<TEntity> where TEntity : IIdentifiableEntity
    {
        public QueryResults(List<TEntity> results)
        {
            Results = results;
        }

        public List<TEntity> Results { get; }
    }
}