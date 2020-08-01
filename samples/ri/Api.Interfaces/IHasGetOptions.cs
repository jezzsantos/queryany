namespace Api.Interfaces
{
    /// <summary>
    ///     Defines options for GET operations
    /// </summary>
    public interface IHasGetOptions
    {
        /// <summary>
        ///     Gets or sets the options embedding child resources
        /// </summary>
        string Embed { get; set; }
    }
}