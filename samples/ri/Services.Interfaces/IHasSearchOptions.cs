namespace Services.Interfaces
{
    /// <summary>
    ///     Defines options for GET operations that performs searches
    /// </summary>
    public interface IHasSearchOptions : IHasGetOptions
    {
        /// <summary>
        ///     Gets or sets the number of results to return
        /// </summary>
        int? Limit { get; set; }

        /// <summary>
        ///     Gets or sets the starting offset of the limited results
        /// </summary>
        int? Offset { get; set; }

        /// <summary>
        ///     Gets or sets the sorting of the results
        /// </summary>
        string Sort { get; set; }

        /// <summary>
        ///     Gets or sets the filtering of the results
        /// </summary>
        string Filter { get; set; }

        /// <summary>
        ///     Gets or sets the distinct field
        /// </summary>
        string Distinct { get; set; }
    }
}