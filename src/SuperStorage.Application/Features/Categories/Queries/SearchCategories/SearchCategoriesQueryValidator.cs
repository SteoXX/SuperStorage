using FluentValidation;

namespace SuperStorage.Application.Features.Categories.Queries.SearchCategories;

internal sealed class SearchCategoriesQueryValidator : AbstractValidator<SearchCategoriesQuery>
{
    public SearchCategoriesQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.SearchTerm)
            .MaximumLength(120);
    }
}
