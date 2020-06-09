using QueryAny;
using Services.Interfaces;
using ServiceStack.FluentValidation;

namespace CarsApi.Validators
{
    public interface IHasSearchOptionsValidator : IValidator<IHasSearchOptions>
    {

    }

    public class HasSearchOptionsValidator : AbstractValidator<IHasSearchOptions>, IHasSearchOptionsValidator
    {
        public const string SortExpression = @"^((([\;\,]{0,1})([\+\-]{0,1})([\d\w\._]{1,25})){1,5})$";
        public const string FilterExpression = @"^((([\;\,]{0,1})([\d\w\._]{1,25})){1,25})$";
        public const string DistinctExpression = @"^[\d\w\._]{1,25}$";

        public HasSearchOptionsValidator(IHasGetOptionsValidator hasGetOptionsValidator)
        {
            When(dto => dto.Limit.HasValue, () =>
            {
                RuleFor(dto => dto.Limit.Value).InclusiveBetween(SearchOptions.NoLimit, SearchOptions.MaxLimit)
                    .WithMessage(Properties.Resources.HasSearchOptionsValidator_InvalidLimit.Format(SearchOptions.NoLimit, SearchOptions.DefaultLimit));
            });
            When(dto => dto.Offset.HasValue, () =>
            {
                RuleFor(dto => dto.Offset.Value).InclusiveBetween(SearchOptions.NoOffset, SearchOptions.MaxLimit)
                    .WithMessage(Properties.Resources.HasSearchOptionsValidator_InvalidOffset.Format(SearchOptions.NoOffset, SearchOptions.MaxLimit));
            });
            When(dto => dto.Sort.HasValue(), () =>
            {
                RuleFor(dto => dto.Sort).Matches(SortExpression)
                    .WithMessage(Properties.Resources.HasSearchOptionsValidator_InvalidSort);
            });
            When(dto => dto.Filter.HasValue(), () =>
            {
                RuleFor(dto => dto.Filter).Matches(FilterExpression)
                    .WithMessage(Properties.Resources.HasSearchOptionsValidator_InvalidFilter);
            });
            When(dto => dto.Distinct.HasValue(), () =>
            {
                RuleFor(dto => dto.Distinct).Matches(DistinctExpression)
                    .WithMessage(Properties.Resources.HasSearchOptionsValidator_InvalidDistinct);
            });
            When(dto => dto.Embed.HasValue(), () =>
            {
                RuleFor(dto => dto).SetValidator(hasGetOptionsValidator);
            });

        }
    }

}
