namespace Api.Interfaces
{
    /// <summary>
    ///     Defines options for GET operations that performs searches
    /// </summary>
    public interface IHasSearchOptions : IHasGetOptions
    {
        int? Limit { get; set; }

        int? Offset { get; set; }

        string Sort { get; set; }

        string Filter { get; set; }
    }
}