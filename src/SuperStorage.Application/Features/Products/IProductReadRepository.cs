using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Products;

public interface IProductReadRepository : IReadRepository<Product, Guid>
{
    Task<ProductResponse?> GetByIdResponseAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ProductListItemResponse>> SearchAsync(
        string? searchTerm,
        Guid? categoryId,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
