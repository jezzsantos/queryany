using System.Linq;
using QueryAny;
using Services.Interfaces;
using ServiceStack.FluentValidation;

namespace CarsApi.Validators
{
    public interface IHasGetOptionsValidator : IValidator<IHasGetOptions>
    {

    }

    public class HasGetOptions : AbstractValidator<IHasGetOptions>, IHasGetOptionsValidator
    {
        public const string ResourceReferenceExpression = @"^[\d\w\.]{1,100}$";

        public HasGetOptions()
        {
            When(dto => dto.Embed.HasValue(), () =>
            {
                RuleForEach(dto => dto.ToGetOptions(null, null).ResourceReferences)
                    .Matches(ResourceReferenceExpression)
                    .WithMessage("The format of the Embed expression in invalid");
                RuleFor(dto => dto.ToGetOptions(null, null).ResourceReferences.Count()).LessThanOrEqualTo(GetOptions.MaxResourceReferences)
                    .WithMessage($"The Embed expression contains too many resources. Maximum of {GetOptions.MaxResourceReferences} allowed");
            });
        }
    }

}
