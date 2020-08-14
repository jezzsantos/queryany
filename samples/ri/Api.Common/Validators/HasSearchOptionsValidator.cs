using Api.Common.Properties;
using Api.Interfaces;
using Domain.Interfaces;
using QueryAny.Primitives;
using ServiceStack.FluentValidation;

namespace Api.Common.Validators
{
    public interface IHasSearchOptionsValidator : IValidator<IHasSearchOptions>
    {
    }

    public class HasSearchOptionsValidator : AbstractValidator<IHasSearchOptions>, IHasSearchOptionsValidator
    {
        private const string SortExpression = @"^((([\;\,]{0,1})([\+\-]{0,1})([\d\w\._]{1,25})){1,5})$";
        private const string FilterExpression = @"^((([\;\,]{0,1})([\d\w\._]{1,25})){1,25})$";

        public HasSearchOptionsValidator(IHasGetOptionsValidator hasGetOptionsValidator)
        {
            When(dto => dto.Limit.HasValue, () =>
            {
                RuleFor(dto => dto.Limit.Value).InclusiveBetween(SearchOptions.NoLimit, SearchOptions.MaxLimit)
                    .WithMessage(Resources.HasSearchOptionsValidator_InvalidLimit.Format(SearchOptions.NoLimit,
                        SearchOptions.DefaultLimit));
            });
            When(dto => dto.Offset.HasValue, () =>
            {
                RuleFor(dto => dto.Offset.Value).InclusiveBetween(SearchOptions.NoOffset, SearchOptions.MaxLimit)
                    .WithMessage(Resources.HasSearchOptionsValidator_InvalidOffset.Format(SearchOptions.NoOffset,
                        SearchOptions.MaxLimit));
            });
            When(dto => dto.Sort.HasValue(), () =>
            {
                RuleFor(dto => dto.Sort).Matches(SortExpression)
                    .WithMessage(Resources.HasSearchOptionsValidator_InvalidSort);
            });
            When(dto => dto.Filter.HasValue(), () =>
            {
                RuleFor(dto => dto.Filter).Matches(FilterExpression)
                    .WithMessage(Resources.HasSearchOptionsValidator_InvalidFilter);
            });
            When(dto => dto.Embed.HasValue(), () => { RuleFor(dto => dto).SetValidator(hasGetOptionsValidator); });
        }
    }
}