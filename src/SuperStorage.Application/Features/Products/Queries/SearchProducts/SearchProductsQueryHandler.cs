using MediatR;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products.Queries.SearchProducts;

internal sealed class SearchProductsQueryHandler(IQueryDbContext dbContext)
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductListItemResponse>>
{
    public async Task<PagedResult<ProductListItemResponse>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Query<Product>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToUpperInvariant();

            var hasSku = TryCreateSku(searchTerm, out var sku);

            query = query.Where(product =>
                product.Code.ToUpper().Contains(searchTerm) ||
                (hasSku && product.Sku == sku));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(product => product.IsActive == request.IsActive.Value);
        }

        var totalCount = await dbContext.CountAsync(query, cancellationToken);

        var products = await dbContext.ToListAsync(
            query
                .OrderBy(product => product.Code)
                .ThenBy(product => product.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(product => new ProductListItemResponse(
                    product.Id,
                    product.Code,
                    product.Sku.Value,
                    product.CategoryId,
                    product.Category == null ? null : product.Category.Name,
                    product.Description,
                    product.IsActive,
                    product.CreatedAtUtc,
                    product.UpdatedAtUtc)),
            cancellationToken);

        return new PagedResult<ProductListItemResponse>(
            products,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }

    private static bool TryCreateSku(string value, out Sku sku)
    {
        try
        {
            sku = Sku.Create(value);
            return true;
        }
        catch (ArgumentException)
        {
            sku = null!;
            return false;
        }
    }

}
