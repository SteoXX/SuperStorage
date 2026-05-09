using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Features.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(WmsDbContext dbContext)
    : Repository<Product, Guid>(dbContext), IProductRepository
{
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
}
