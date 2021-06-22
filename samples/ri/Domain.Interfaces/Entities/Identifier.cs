using System.Collections.Generic;
using System.Linq;
using Common;
using ServiceStack;

namespace Domain.Interfaces.Entities
{
    public class Identifier : SingleValueObjectBase<Identifier, string>
    {
        private Identifier() : base(string.Empty)
        {
        }

        private Identifier(string identifier) : base(identifier)
        {
            identifier.GuardAgainstNullOrEmpty(nameof(identifier));
            AutoMapping.RegisterConverter((Identifier valueObject) => valueObject?.Value);
        }

        public static Identifier Empty()
        {
            return new Identifier();
        }

        public bool IsEmpty()
        {
            return !Value.HasValue();
        }

        public static Identifier Create(string value)
        {
            return new Identifier(value);
        }

        public static ValueObjectFactory<Identifier> Rehydrate()
        {
            return (property, container) => new Identifier(property);
        }
    }

    public static class IdentifierExtensions
    {
        public static Identifier ToIdentifier(this string id)
        {
            return Identifier.Create(id);
        }

        public static List<Identifier> ToIdentifiers(this List<string> ids)
        {
            return ids.Select(id => id.ToIdentifier()).ToList();
        }
    }
}