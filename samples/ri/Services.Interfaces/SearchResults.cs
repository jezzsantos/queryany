using System.Collections.Generic;

namespace Services.Interfaces
{
    public class SearchResults<TResource>
    {
        public SearchResults()
        {
            Metadata = new SearchMetadata();
            Results = new List<TResource>();
        }

        public List<TResource> Results { get; set; }

        public SearchMetadata Metadata { get; set; }
    }
}