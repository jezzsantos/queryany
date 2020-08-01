namespace Api.Interfaces
{
    public class HasSearchOptions : IHasSearchOptions
    {
        public string Distinct { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }

        public string Embed { get; set; }
    }
}