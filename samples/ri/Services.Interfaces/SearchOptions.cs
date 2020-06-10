using System.Collections.Generic;

namespace Services.Interfaces
{
    public class SearchOptions
    {
        internal const char SortSignAscending = '+';
        internal const char SortSignDescending = '-';

        public const int DefaultLimit = 100;
        public const int MaxLimit = 1000;
        public const int NoLimit = 0;
        public const int NoOffset = -1;

        internal static readonly char[] FilterDelimiters =
        {
            ',',
            ';'
        };

        internal static readonly char[] SortSigns =
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

        public string Distinct { get; set; }
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