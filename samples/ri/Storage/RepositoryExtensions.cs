using System;
using System.Collections.Generic;
using System.Linq;
using QueryAny;
using ServiceStack;

namespace Storage
{
    public static class RepositoryExtensions
    {
        public static List<TEntity> GetJoinedResults<TEntity>(this JoinDefinition joinDefinition,
            Dictionary<string, Dictionary<string, object>> leftEntities,
            Dictionary<string, Dictionary<string, object>> rightEntities)
        {
            switch (joinDefinition.Type)
            {
                case JoinType.Inner:
                case JoinType.Right:
                    var innerJoin = from lefts in leftEntities
                        join rights in rightEntities on lefts.Value[joinDefinition.Left.JoinedFieldName] equals
                            rights.Value[joinDefinition.Right.JoinedFieldName]
                        select lefts;
                    return innerJoin
                        .Select(e => e.Value.FromObjectDictionary<TEntity>())
                        .ToList();

                case JoinType.Outer:
                case JoinType.Left:
                    var leftJoin = from lefts in leftEntities
                        join rights in rightEntities on lefts.Value[joinDefinition.Left.JoinedFieldName] equals
                            rights.Value[joinDefinition.Right.JoinedFieldName]
                            into joined
                        from result in joined.DefaultIfEmpty()
                        select lefts;
                    return leftJoin
                        .Select(e => e.Value.FromObjectDictionary<TEntity>())
                        .ToList();

                default:
                    throw new ArgumentOutOfRangeException(nameof(JoinType));
            }
        }
    }
}