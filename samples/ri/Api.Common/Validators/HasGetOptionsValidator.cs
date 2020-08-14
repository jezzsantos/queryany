using System.Linq;
using Api.Common.Properties;
using Api.Interfaces;
using Domain.Interfaces;
using QueryAny.Primitives;
using ServiceStack.FluentValidation;

namespace Api.Common.Validators
{
    public interface IHasGetOptionsValidator : IValidator<IHasGetOptions>
    {
    }

    public class HasGetOptionsValidator : AbstractValidator<IHasGetOptions>, IHasGetOptionsValidator
    {
        public const string ResourceReferenceExpression = @"^[\d\w\.]{1,100}$";

        public HasGetOptionsValidator()
        {
            When(dto => dto.Embed.HasValue(), () =>
            {
                RuleForEach(dto => dto.ToGetOptions(null, null).ResourceReferences)
                    .Matches(ResourceReferenceExpression)
                    .WithMessage(Resources.HasGetOptionsValidator_InvalidEmbed);
                RuleFor(dto => dto.ToGetOptions(null, null).ResourceReferences.Count())
                    .LessThanOrEqualTo(GetOptions.MaxResourceReferences)
                    .WithMessage(
                        Resources.HasGetOptionsValidator_TooManyResourceReferences.Format(GetOptions
                            .MaxResourceReferences));
            });
        }
    }
}