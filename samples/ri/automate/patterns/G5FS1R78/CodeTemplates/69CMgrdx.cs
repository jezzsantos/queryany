using System;
using Domain.Interfaces;

namespace {{DomainName | string.pascalplural}}Domain
{
    public static class Validations
    {
        public static class {{DomainName | string.pascalsingular}}
        {
            public static readonly ValidationFormat AName = new ValidationFormat(@"^[\d\w\-\. ]{1,50}$", 1, 50);
            public static readonly ValidationFormat ADescription = Domain.Interfaces.Validations.DescriptiveName(1, 50);
        }
    }
}