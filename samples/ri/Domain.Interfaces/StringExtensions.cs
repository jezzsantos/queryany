using Domain.Interfaces.Entities;

namespace Domain.Interfaces
{
    public static class StringExtensions
    {
        public static IIdentifierFactory ToIdentifierFactory(this string identifier)
        {
            return new FixedIdentifierFactory(identifier);
        }

        public static IIdentifierFactory ToIdentifierFactory(this Identifier identifier)
        {
            return new FixedIdentifierFactory(identifier);
        }
    }
}