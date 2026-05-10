using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Features.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Infrastructure.Persistence.Repositories.Products;

internal sealed class ProductRepository(SuperStorageDbContext dbContext)
    : Repository<Product, Guid>(dbContext), IProductRepository
{
    public async Task<Product?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Product>()
            .SingleOrDefaultAsync(product => product.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Product>()
            .AnyAsync(product => product.Code == code, cancellationToken);
    }

    public async Task<Product?> GetBySkuAsync(
        Sku sku,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Product>()
            .SingleOrDefaultAsync(product => product.Sku == sku, cancellationToken);
    }

    public async Task<bool> ExistsBySkuAsync(
        Sku sku,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Product>()
            .AnyAsync(product => product.Sku == sku, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Product>> GetByCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Product>()
            .Where(product => product.CategoryId == categoryId)
            .ToListAsync(cancellationToken);
    }
}
