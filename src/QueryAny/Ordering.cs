namespace QueryAny
{
    public class Ordering
    {
        public Ordering(string by, OrderDirection direction)
        {
            By = by;
            Direction = direction;
        }

        public string By { get; }
        public OrderDirection Direction { get; }
    }

    public enum OrderDirection
    {
        Ascending = 0,
        Descending = 1
    }
}