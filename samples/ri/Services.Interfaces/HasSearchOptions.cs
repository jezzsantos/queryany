namespace Services.Interfaces
{
    public class HasSearchOptions : IHasSearchOptions
    {
        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }

        public string Distinct { get; set; }

        public string Embed { get; set; }
    }
}
