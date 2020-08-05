using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using QueryAny.Primitives;

namespace Api.Interfaces
{
    public static class HasSearchOptionsExtensions
    {
        public static SearchOptions ToSearchOptions(this IHasSearchOptions options, int? defaultLimit = null,
            int? defaultOffset = null, string defaultSort = null,
            string defaultFilter = null)
        {
            if (options == null)
            {
                return null;
            }

            var result = new SearchOptions
            {
                Limit = options.Limit.HasValue
                    ? options.Limit.Value > SearchOptions.NoLimit
                        ? options.Limit.Value
                        : SearchOptions.DefaultLimit
                    : defaultLimit.HasValue
                        ? defaultLimit.Value > SearchOptions.NoLimit
                            ? defaultLimit.Value
                            : SearchOptions.DefaultLimit
                        : SearchOptions.DefaultLimit,
                Offset = options.Offset ?? (defaultOffset ?? SearchOptions.NoOffset)
            };

            if (options.Sort.HasValue())
            {
                result.Sort.By = ParseSortBy(options.Sort);
                result.Sort.Direction = ParseSortDirection(options.Sort);
            }
            else
            {
                if (defaultSort.HasValue())
                {
                    result.Sort.By = ParseSortBy(defaultSort);
                    result.Sort.Direction = ParseSortDirection(defaultSort);
                }
            }

            if (options.Filter.HasValue())
            {
                result.Filter.Fields = ParseFilters(options.Filter);
            }
            else
            {
                if (defaultFilter.HasValue())
                {
                    result.Filter.Fields = ParseFilters(defaultFilter);
                }
            }

            return result;
        }

        private static string ParseSortBy(string sortBy)
        {
            return sortBy.StartsWith(SearchOptions.SortSigns[0].ToString()) ||
                   sortBy.StartsWith(SearchOptions.SortSigns[1].ToString())
                ? sortBy.TrimStart(SearchOptions.SortSigns)
                : sortBy;
        }

        private static SortDirection ParseSortDirection(string sort)
        {
            return sort.StartsWith(SearchOptions.SortSignDescending.ToString())
                ? SortDirection.Descending
                : SortDirection.Ascending;
        }

        private static List<string> ParseFilters(string filter)
        {
            return filter.Split(SearchOptions.FilterDelimiters).ToList();
        }
    }
}