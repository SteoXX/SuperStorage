using SuperStorage.Application.Abstractions.Messaging;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;

namespace SuperStorage.Application.Features.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(
    string? SearchTerm,
    Guid? CategoryId,
    bool? IsActive,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResult<ProductListItemResponse>>;
