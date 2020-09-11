using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    public abstract class EntityPrefixIdentifierFactory : IIdentifierFactory
    {
        private const string UnknownEntityPrefix = "xxx";
        private const string Delimiter = "_";
        private readonly Dictionary<Type, string> prefixes;

        protected EntityPrefixIdentifierFactory(Dictionary<Type, string> prefixes)
        {
            prefixes.GuardAgainstNull(nameof(prefixes));

            this.prefixes = new Dictionary<Type, string>(prefixes) {{typeof(EntityEvent), "evt"}};
        }

        public Identifier Create(IIdentifiableEntity entity)
        {
            var random = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", string.Empty)
                .Replace("/", string.Empty)
                .Replace("=", string.Empty);

            var entityType = entity.GetType();
            if (this.prefixes.ContainsKey(entityType))
            {
                var prefix = this.prefixes[entity.GetType()];
                return $"{prefix}{Delimiter}{random}".ToIdentifier();
            }

            return $"{UnknownEntityPrefix}{Delimiter}{random}".ToIdentifier();
        }

        public bool IsValid(Identifier value)
        {
            if (!value.HasValue())
            {
                return false;
            }

            var id = value.ToString();
            var delimiterIndex = id.IndexOf(Delimiter, StringComparison.Ordinal);
            if (delimiterIndex == -1)
            {
                return false;
            }

            var prefix = id.Substring(0, delimiterIndex);
            if (!this.prefixes.ContainsValue(prefix)
                && prefix != UnknownEntityPrefix)
            {
                return false;
            }

            var suffix = id.Substring(delimiterIndex + 1);

            return Regex.IsMatch(suffix, @"^[\d\w]{10,22}$");
        }
    }
}