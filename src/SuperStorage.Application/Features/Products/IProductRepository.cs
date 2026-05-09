using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetBySkuAsync(Sku sku, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
}
