using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Infrastructure.Persistence.Repositories.Categories;

internal sealed class CategoryReadRepository(IReadDbContext dbContext)
    : ReadRepository<Category, Guid>(dbContext), ICategoryReadRepository
{
    private const int ProductPreviewCount = 5;

    public async Task<CategoryResponse?> GetByIdResponseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(category => category.Id == id)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.Description,
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<CategoryListItemResponse>> SearchAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Query();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearchTerm = searchTerm.Trim().ToUpperInvariant();

            query = query.Where(category =>
                category.Name.ToUpper().Contains(normalizedSearchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(category => category.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var categories = await query
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(category => new CategoryListItemResponse(
                category.Id,
                category.Name,
                category.Description,
                category.IsActive,
                category.CreatedAtUtc,
                category.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<CategoryListItemResponse>(
            categories,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<CategoryDeleteImpactResponse?> GetDeleteImpactAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await Query()
            .Where(category => category.Id == id)
            .Select(category => new
            {
                category.Id,
                category.Name
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            return null;
        }

        var linkedProductsQuery = ReadDbContext.Query<Product>()
            .Where(product => product.CategoryId == category.Id);

        var linkedProductsCount = await linkedProductsQuery.CountAsync(cancellationToken);

        var linkedProducts = await linkedProductsQuery
            .OrderBy(product => product.Code)
            .ThenBy(product => product.Id)
            .Take(ProductPreviewCount)
            .Select(product => new CategoryLinkedProductResponse(
                product.Id,
                product.Code,
                product.Sku.Value))
            .ToListAsync(cancellationToken);

        return new CategoryDeleteImpactResponse(
            category.Id,
            category.Name,
            linkedProductsCount,
            linkedProducts);
    }

    public async Task<IReadOnlyCollection<CategoryLookupResponse>> GetActiveLookupsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new CategoryLookupResponse(
                category.Id,
                category.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .AnyAsync(category => category.Id == id, cancellationToken);
    }

    public async Task<string?> GetNameByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(category => category.Id == id)
            .Select(category => category.Name)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
