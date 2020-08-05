using System;
using System.Linq.Expressions;
using Domain.Interfaces;

namespace Api.Interfaces
{
    public class HasGetOptions : IHasGetOptions
    {
        public const string EmbedAll = "*";
        public const string EmbedNone = "off";

        public static readonly HasGetOptions All = new HasGetOptions {Embed = EmbedAll};
        public static readonly HasGetOptions None = new HasGetOptions {Embed = EmbedNone};

        public string Embed { get; set; }

        public static HasGetOptions Custom<TResource>(params Expression<Func<TResource, object>>[] resourceProperties)
        {
            return GetOptions.Custom(resourceProperties).ToHasGetOptions();
        }
    }
}