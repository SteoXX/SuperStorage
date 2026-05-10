using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Categories;
using SuperStorage.Contracts.Common;
using SuperStorage.Contracts.Products;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories;

public interface ICategoryReadRepository : IReadRepository<Category, Guid>
{
    Task<CategoryResponse?> GetByIdResponseAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CategoryListItemResponse>> SearchAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CategoryDeleteImpactResponse?> GetDeleteImpactAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CategoryLookupResponse>> GetActiveLookupsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<string?> GetNameByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
