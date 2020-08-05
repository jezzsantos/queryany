using ServiceStack;

namespace Domain.Interfaces
{
    public static class SearchOptionExtensions
    {
        public static SearchMetadata ToMetadataSafe(this SearchOptions options, int total = 0)
        {
            if (options == null)
            {
                return ToMetadataSafe(new SearchOptions(), total);
            }

            var metadata = options.ConvertTo<SearchMetadata>();
            metadata.Total = total;

            return metadata;
        }
    }
}