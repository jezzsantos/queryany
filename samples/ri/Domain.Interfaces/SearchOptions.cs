using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using QueryAny.Primitives;

namespace Domain.Interfaces
{
    public class SearchOptions
    {
        public const char SortSignAscending = '+';
        public const char SortSignDescending = '-';
        public const int DefaultLimit = 100;
        public const int MaxLimit = 1000;
        public const int NoLimit = 0;
        public const int NoOffset = -1;
        public static readonly char[] FilterDelimiters =
        {
            ',',
            ';'
        };
        public static readonly char[] SortSigns =
        {
            SortSignAscending,
            SortSignDescending
        };

        public static SearchOptions WithMaxLimit = new SearchOptions {Limit = MaxLimit};

        public SearchOptions()
        {
            Offset = NoOffset;
            Limit = DefaultLimit;
            Sort = new Sorting();
            Filter = new Filtering();
        }

        public int Limit { get; set; }

        public int Offset { get; set; }

        public Sorting Sort { get; set; }

        public Filtering Filter { get; set; }

        public SearchResults<TResult> ApplyWithMetadata<TResult>(IEnumerable<TResult> results)
        {
            return ApplyWithMetadata(results, SearchOptions<TResult>.DynamicOrderByFunc);
        }

        public void ClearLimitAndOffset()
        {
            Offset = NoOffset;
            Limit = DefaultLimit;
        }

        private SearchResults<TResult> ApplyWithMetadata<TResult>(IEnumerable<TResult> results,
            Func<IEnumerable<TResult>, Sorting, IEnumerable<TResult>> orderByFunc)
        {
            var searchResults = new SearchResults<TResult>
            {
                Metadata = this.ToMetadataSafe()
            };

            var unsorted = results.ToList();
            searchResults.Metadata.Total = unsorted.Count();

            if (IsSorted())
            {
                unsorted = orderByFunc(unsorted, Sort).ToList();
            }

            IEnumerable<TResult> unPaged = unsorted.ToArray();

            if (IsOffSet())
            {
                unPaged = unPaged.Skip(Offset);
            }

            if (IsLimited())
            {
                var limit = Math.Min(MaxLimit, Limit);
                unPaged = unPaged.Take(limit);
            }
            else
            {
                unPaged = unPaged.Take(DefaultLimit);
            }

            searchResults.Results = unPaged.ToList();

            return searchResults;
        }

        private bool IsLimited()
        {
            return Limit > NoLimit;
        }

        private bool IsOffSet()
        {
            return Offset > NoOffset;
        }

        private bool IsSorted()
        {
            return Sort?.By != null && Sort.By.HasValue();
        }
    }

    public static class SearchOptions<TResult>
    {
        public static readonly Func<IEnumerable<TResult>, Sorting, IEnumerable<TResult>> DynamicOrderByFunc =
            (items, sorting) =>
            {
                var by = sorting.By;

                string expression = null;
                if (sorting.Direction == SortDirection.Ascending)
                {
                    expression = $"{by} ascending";
                }

                if (sorting.Direction == SortDirection.Descending)
                {
                    expression = $"{by} descending";
                }

                try
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    return items.AsQueryable().OrderBy(expression);
                }
                catch (ParseException)
                {
                    // Ignore exception. Possibly an invalid sorting expression?
                    // ReSharper disable once PossibleMultipleEnumeration
                    return items;
                }
            };
    }

    public class Sorting
    {
        public Sorting()
        {
            Direction = SortDirection.Ascending;
        }

        public string By { get; set; }

        public SortDirection Direction { get; set; }

        public static Sorting ByField(string field, SortDirection direction = SortDirection.Ascending)
        {
            return new Sorting
            {
                By = field,
                Direction = direction
            };
        }
    }

    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }

    public class Filtering
    {
        public Filtering()
        {
            Fields = new List<string>();
        }

        public List<string> Fields { get; set; }
    }
}