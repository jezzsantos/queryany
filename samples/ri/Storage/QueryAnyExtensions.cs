using System.Linq;
using Common;
using QueryAny;

namespace Storage
{
    public static class QueryAnyExtensions
    {
        public static bool HasAnyJoins<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IQueryableEntity
        {
            query.GuardAgainstNull(nameof(query));

            return query.JoinedEntities
                .Any(je => je.Join.Exists());
        }
    }
}