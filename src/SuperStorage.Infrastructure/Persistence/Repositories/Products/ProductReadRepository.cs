using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Application.Features.Products;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Infrastructure.Persistence.Repositories.Products;

internal sealed class ProductReadRepository(IReadDbContext dbContext)
    : ReadRepository<Product, Guid>(dbContext), IProductReadRepository
{
    public async Task<ProductResponse?> GetByIdResponseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(product => product.Id == id)
            .Select(product => new ProductResponse(
                product.Id,
                product.Code,
                product.Sku.Value,
                product.CategoryId,
                product.CategoryId.HasValue ? product.Category!.Name : null,
                product.Description,
                product.IsActive,
                product.CreatedAtUtc,
                product.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ProductListItemResponse>> SearchAsync(
        string? searchTerm,
        Guid? categoryId,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Query();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim().ToUpperInvariant();
            var hasSku = TryCreateSku(normalizedSearchTerm, out var sku);

            query = query.Where(product =>
                product.Code.ToUpper().Contains(normalizedSearchTerm) ||
                (hasSku && product.Sku == sku));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(product => product.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(product => product.Code)
            .ThenBy(product => product.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductListItemResponse(
                product.Id,
                product.Code,
                product.Sku.Value,
                product.CategoryId,
                product.CategoryId.HasValue ? product.Category!.Name : null,
                product.Description,
                product.IsActive,
                product.CreatedAtUtc,
                product.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemResponse>(
            products,
            pageNumber,
            pageSize,
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
