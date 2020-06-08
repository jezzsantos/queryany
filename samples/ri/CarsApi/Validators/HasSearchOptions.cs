using QueryAny;
using Services.Interfaces;
using ServiceStack.FluentValidation;

namespace CarsApi.Validators
{
    public interface IHasSearchOptionsValidator : IValidator<IHasSearchOptions>
    {

    }

    public class HasSearchOptions : AbstractValidator<IHasSearchOptions>, IHasSearchOptionsValidator
    {
        public const string SortExpression = @"^((([\;\,]{0,1})([\+\-]{0,1})([\d\w\._]{1,25})){1,5})$";
        public const string FilterExpression = @"^((([\;\,]{0,1})([\d\w\._]{1,25})){1,25})$";
        public const string DistinctExpression = @"^[\d\w\._]{1,25}$";

        public HasSearchOptions(IHasGetOptionsValidator hasGetOptionsValidator)
        {
            When(dto => dto.Limit.HasValue, () =>
            {
                RuleFor(dto => dto.Limit.Value).InclusiveBetween(SearchOptions.NoLimit, SearchOptions.MaxLimit)
                    .WithMessage($"The Limt for this operation must be between {SearchOptions.NoLimit} and {SearchOptions.MaxLimit}");
            });
            When(dto => dto.Offset.HasValue, () =>
            {
                RuleFor(dto => dto.Offset.Value).InclusiveBetween(SearchOptions.NoOffset, SearchOptions.MaxLimit)
                    .WithMessage($"The offset must be between {SearchOptions.NoOffset} and {SearchOptions.MaxLimit}");
            });
            When(dto => dto.Sort.HasValue(), () =>
            {
                RuleFor(dto => dto.Sort).Matches(SortExpression)
                    .WithMessage($"The format of the Sort expression is invalid");
            });
            When(dto => dto.Filter.HasValue(), () =>
            {
                RuleFor(dto => dto.Filter).Matches(FilterExpression)
                    .WithMessage("The format of the Filter expression is invalid");
            });
            When(dto => dto.Distinct.HasValue(), () =>
            {
                RuleFor(dto => dto.Distinct).Matches(DistinctExpression)
                    .WithMessage("The format of the Distinct expression is invalid");
            });
            When(dto => dto.Embed.HasValue(), () =>
            {
                RuleFor(dto => dto).SetValidator(hasGetOptionsValidator);
            });

        }
    }

}
