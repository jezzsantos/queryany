namespace Services.Interfaces.Apis
{
    public abstract class SearchOperation<TResponse> : GetOperation<TResponse>, IHasSearchOptions
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public string Sort { get; set; }
        public string Filter { get; set; }
        public string Distinct { get; set; }
    }
}