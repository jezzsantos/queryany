using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using QueryAny.Primitives;

namespace Api.Interfaces
{
    public static class HasGetOptionsExtensions
    {
        /// <summary>
        ///     Converts the specified <see cref="IHasGetOptions" /> to a <see cref="GetOptions" />
        /// </summary>
        public static GetOptions ToGetOptions(this IHasGetOptions options,
            ExpandOptions? defaultExpand = ExpandOptions.All, List<string> defaultChildResources = null)
        {
            if (options == null)
            {
                return null;
            }

            var embedValue = options.Embed;
            if (!embedValue.HasValue())
            {
                if (defaultChildResources != null && defaultChildResources.Any())
                {
                    return GetOptions.Custom(defaultChildResources);
                }

                return new GetOptions(defaultExpand ?? ExpandOptions.All);
            }

            if (embedValue.EqualsIgnoreCase(HasGetOptions.EmbedNone))
            {
                return GetOptions.None;
            }

            if (embedValue.EqualsIgnoreCase(HasGetOptions.EmbedAll))
            {
                return GetOptions.All;
            }

            var values = options.Embed
                .SafeSplit(GetOptions.EmbedRequestParamDelimiter)
                .Select(value => value.ToLowerInvariant().Trim())
                .ToList();

            return GetOptions.Custom(values);
        }

        public static HasGetOptions ToHasGetOptions(this GetOptions options)
        {
            if (options == null)
            {
                return null;
            }

            var childResourcesList = options.ResourceReferences != null && options.ResourceReferences.Any()
                ? options.ResourceReferences.Join(GetOptions.EmbedRequestParamDelimiter)
                : null;

            return new HasGetOptions
            {
                Embed = options.Expand == ExpandOptions.Custom
                    ? childResourcesList
                    : null
            };
        }
    }
}