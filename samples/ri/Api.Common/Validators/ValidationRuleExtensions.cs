using Domain.Interfaces.Entities;
using QueryAny.Primitives;
using ServiceStack.FluentValidation;

namespace Api.Common.Validators
{
    public static class ValidationRuleExtensions
    {
        public static IRuleBuilderOptions<TDto, string> IsEntityId<TDto>(this IRuleBuilderInitial<TDto, string> rule,
            IIdentifierFactory identifierFactory)
        {
            return rule.Must(id => id.HasValue() && identifierFactory.IsValid(Identifier.Create(id)));
        }
    }
}