using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using QueryAny;
using QueryAny.Primitives;

namespace Domain.Interfaces
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
                var propertyName = options.Sort.By;
                var propertyExpression = GetPropertyExpression<TEntity>(propertyName);
                if (propertyExpression != null)
                {
                    query.OrderBy(propertyExpression, options.Sort.Direction == SortDirection.Ascending
                        ? OrderDirection.Ascending
                        : OrderDirection.Descending);
                }
            }

            if (options.Filter.Fields.Any())
            {
                foreach (var field in options.Filter.Fields)
                {
                    var propertyExpression = GetPropertyExpression<TEntity>(field);
                    if (propertyExpression != null)
                    {
                        query.Select(propertyExpression);
                    }
                }
            }

            return query;
        }

        private static Expression<Func<TEntity, object>> GetPropertyExpression<TEntity>(string propertyName)
            where TEntity : IQueryableEntity
        {
            var propertyInfo = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(info => info.Name.EqualsIgnoreCase(propertyName));
            if (propertyInfo == null)
            {
                return null;
            }

            var variable = Expression.Parameter(typeof(TEntity));
            var propertySelector = Expression.Property(variable, propertyInfo);

            return Expression.Lambda<Func<TEntity, object>>(propertySelector, variable);
        }
    }
}