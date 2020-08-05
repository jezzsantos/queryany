using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public class GetOptions
    {
        public const int MaxResourceReferences = 10;
        public const string EmbedRequestParamDelimiter = ",";
        public const string EmbedRequestParamName = "embed";

        public static GetOptions All = new GetOptions(ExpandOptions.All, new List<string>());
        public static GetOptions None = new GetOptions(ExpandOptions.None, new List<string>());
        private readonly List<string> resourceReferences;

        public GetOptions()
            : this(ExpandOptions.All)
        {
        }

        public GetOptions(ExpandOptions expand, List<string> childReferences = null, ExpandOptions? @default = null)
        {
            Initial = @default ?? ExpandOptions.All;
            Expand = expand;
            this.resourceReferences = childReferences ?? new List<string>();
        }

        public ExpandOptions Expand { get; }

        public ExpandOptions Initial { get; }

        public IEnumerable<string> ResourceReferences => this.resourceReferences;

        /// <summary>
        ///     Creates a custom set of options for the specified resource properties
        /// </summary>
        public static GetOptions Custom(List<string> resourceReferences)
        {
            return new GetOptions(ExpandOptions.Custom, resourceReferences);
        }

        /// <summary>
        ///     Creates a custom set of options for the specified properties of the specific <see cref="TResource" />
        /// </summary>
        public static GetOptions Custom<TResource>(params Expression<Func<TResource, object>>[] propertyReferences)
        {
            return Custom(propertyReferences.ReferencesToNames());
        }
    }

    public enum ExpandOptions
    {
        None = 0,
        Custom = 1,
        All = 2
    }
}