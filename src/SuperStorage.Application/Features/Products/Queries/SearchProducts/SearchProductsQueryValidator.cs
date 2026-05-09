using FluentValidation;

namespace SuperStorage.Application.Features.Products.Queries.SearchProducts;

internal sealed class SearchProductsQueryValidator : AbstractValidator<SearchProductsQuery>
{
    public SearchProductsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.SearchTerm)
            .MaximumLength(200);
    }
}
