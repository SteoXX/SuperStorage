using MediatR;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Queries.SearchProducts;

internal sealed class SearchProductsQueryHandler(IProductReadRepository productReadRepository)
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductListItemResponse>>
{
    public async Task<PagedResult<ProductListItemResponse>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await productReadRepository.SearchAsync(
            request.SearchTerm,
            request.CategoryId,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
