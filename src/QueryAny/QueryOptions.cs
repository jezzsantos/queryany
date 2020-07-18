namespace QueryAny
{
    public class QueryOptions
    {
        public QueryOptions()
        {
            IsEmpty = false;
        }

        public bool IsEmpty { get; private set; }

        public void SetEmpty()
        {
            IsEmpty = true;
        }
    }
}