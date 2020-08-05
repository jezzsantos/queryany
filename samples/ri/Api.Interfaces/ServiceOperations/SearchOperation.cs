using Domain.Interfaces;

namespace Api.Interfaces.ServiceOperations
{
    public abstract class SearchOperation<TResponse> : GetOperation<TResponse>, IHasSearchOptions
    {
        public string Distinct { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public string Sort { get; set; }

        public string Filter { get; set; }
    }

    public abstract class SearchOperationResponse
    {
        public SearchMetadata Metadata { get; set; }
    }
}