using QueryAny;
using QueryAny.Primitives;

namespace Services.Interfaces
{
    public static class QueryAnyExtensions
    {
        public static QueryClause<TEntity> WithSearchOptions<TEntity>(this QueryClause<TEntity> query,
            SearchOptions options)
            where TEntity : IQueryableEntity
        {
            query.GuardAgainstNull(nameof(query));
            options.GuardAgainstNull(nameof(options));

            if (options.Offset > ResultOptions.DefaultOffset)
            {
                query.Skip(options.Offset);
            }

            if (options.Limit > ResultOptions.DefaultLimit)
            {
                query.Take(options.Limit);
            }

            if (options.Sort.By.HasValue())
            {
                //TODO: how to know what the property name and type will be.
                //TODO: Will need to query the TEntity for the named property 
                query.OrderBy(e => options.Sort.By, options.Sort.Direction == SortDirection.Ascending
                    ? OrderDirection.Ascending
                    : OrderDirection.Descending);
            }

            return query;
        }
    }
}