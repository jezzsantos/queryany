using System.Linq;
using CarsApi.Properties;
using QueryAny;
using Services.Interfaces;
using ServiceStack.FluentValidation;

namespace CarsApi.Validators
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
                    .WithMessage(Properties.Resources.HasGetOptionsValidator_InvalidEmbed);
                RuleFor(dto => dto.ToGetOptions(null, null).ResourceReferences.Count()).LessThanOrEqualTo(GetOptions.MaxResourceReferences)
                    .WithMessage(Resources.HasGetOptionsValidator_TooManyResourceReferences.Format(GetOptions.MaxResourceReferences));
            });
        }
    }

}
