using System.Linq;
using QueryAny.Primitives;

namespace Storage
{
    public static class DynamicLinqExtensions
    {
        public static IQueryable<TEntity> DistinctBy<TEntity>(this IQueryable<TEntity> source, string propertyName)
        {
            if (!propertyName.HasValue())
            {
                return source;
            }

            if (!PropertyExists<TEntity>(propertyName))
            {
                return source;
            }

            var groups = source
                .GroupBy(entity => GetPropertyValue(entity, propertyName));
            if (!groups.Any())
            {
                return source;
            }

            return groups
                .Select(group => group.FirstOrDefault());
        }

        private static bool PropertyExists<TEntity>(string propertyName)
        {
            return typeof(TEntity).GetProperties().Any(property => property.Name.EqualsOrdinal(propertyName));
        }

        private static object GetPropertyValue<TEntity>(TEntity entity, string propertyName)
        {
            var property = entity.GetType().GetProperty(propertyName);
            return property == null
                ? null
                : property.GetValue(entity, null);
        }
    }
}