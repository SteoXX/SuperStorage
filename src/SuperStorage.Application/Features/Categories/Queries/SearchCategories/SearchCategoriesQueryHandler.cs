using MediatR;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;

namespace SuperStorage.Application.Features.Categories.Queries.SearchCategories;

internal sealed class SearchCategoriesQueryHandler(ICategoryReadRepository categoryReadRepository)
    : IRequestHandler<SearchCategoriesQuery, PagedResult<CategoryListItemResponse>>
{
    public async Task<PagedResult<CategoryListItemResponse>> Handle(
        SearchCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await categoryReadRepository.SearchAsync(
            request.SearchTerm,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
