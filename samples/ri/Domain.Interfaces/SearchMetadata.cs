namespace Domain.Interfaces
{
    public class SearchMetadata
    {
        public int Total { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }

        public Sorting Sort { get; set; }

        public Filtering Filter { get; set; }
    }
}