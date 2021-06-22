using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Domain.Interfaces.Entities
{
    public abstract class EntityPrefixIdentifierFactory : IIdentifierFactory
    {
        private const string UnknownEntityPrefix = "xxx";
        private const string Delimiter = "_";
        private readonly Dictionary<Type, string> prefixes;
        private readonly List<string> supportedPrefixes = new List<string>();

        protected EntityPrefixIdentifierFactory(Dictionary<Type, string> prefixes)
        {
            prefixes.GuardAgainstNull(nameof(prefixes));

            this.prefixes = new Dictionary<Type, string>(prefixes) {{typeof(EntityEvent), "evt"}};
        }

        public IEnumerable<Type> RegisteredTypes => this.prefixes.Keys;

        public IReadOnlyList<string> SupportedPrefixes => this.supportedPrefixes;

#if TESTINGONLY
        public Dictionary<string, string> LastCreatedIds { get; } = new Dictionary<string, string>();
#endif

        public void AddSupportedPrefix(string prefix)
        {
            prefix.GuardAgainstNull(nameof(prefix));
            prefix.GuardAgainstInvalid(Validations.IdentifierPrefix, nameof(prefix));

            this.supportedPrefixes.Add(prefix);
        }

        public static string ConvertGuid(Guid guid, string prefix)
        {
            prefix.GuardAgainstNullOrEmpty(nameof(prefix));

            var random = Convert.ToBase64String(guid.ToByteArray())
                .Replace("+", string.Empty)
                .Replace("/", string.Empty)
                .Replace("=", string.Empty);

            return $"{prefix}{Delimiter}{random}".ToIdentifier();
        }

        public Identifier Create(IIdentifiableEntity entity)
        {
            var entityType = entity.GetType();
            var prefix = this.prefixes.ContainsKey(entityType)
                ? this.prefixes[entity.GetType()]
                : UnknownEntityPrefix;

            var guid = Guid.NewGuid();
            var identifier = ConvertGuid(guid, prefix);

#if TESTINGONLY
            LastCreatedIds.Add(identifier, guid.ToString("D"));
#endif

            return identifier.ToIdentifier();
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
            if (!IsKnownPrefix(prefix)
                && prefix != UnknownEntityPrefix)
            {
                return false;
            }

            return Validations.Identifier.Matches(id);
        }

        private bool IsKnownPrefix(string prefix)
        {
            prefix.GuardAgainstNull(nameof(prefix));

            var allPossiblePrefixes = this.prefixes
                .Select(pre => pre.Value)
                .Concat(SupportedPrefixes)
                .Distinct();

            return allPossiblePrefixes.Contains(prefix);
        }
    }
}