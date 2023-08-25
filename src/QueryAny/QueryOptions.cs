namespace QueryAny
{
    public class QueryOptions
    {
        public bool IsEmpty { get; private set; }

        /// <summary>
        ///     Used only internally
        /// </summary>
        internal WhereOptions Wheres { get; set; } = WhereOptions.Undefined;

        public void SetEmpty()
        {
            IsEmpty = true;
        }
    }

    internal enum WhereOptions
    {
        Undefined = 0,
        AllDefined = 1,
        SomeDefined = 2
    }
}