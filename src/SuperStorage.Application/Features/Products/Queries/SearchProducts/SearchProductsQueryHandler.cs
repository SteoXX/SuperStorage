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

            query = TryCreateSku(searchTerm, out var sku)
                ? query.Where(product =>
                    product.Name.ToUpper().Contains(searchTerm) ||
                    product.Sku == sku)
                : query.Where(product => product.Name.ToUpper().Contains(searchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(product => product.IsActive == request.IsActive.Value);
        }

        var totalCount = await dbContext.CountAsync(query, cancellationToken);

        var products = await dbContext.ToListAsync(
            query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize),
            cancellationToken);

        var items = products
            .Select(product => new ProductListItemResponse(
                product.Id,
                product.Sku.Value,
                product.Name,
                product.Description,
                product.IsActive))
            .ToList();

        return new PagedResult<ProductListItemResponse>(
            items,
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
