using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Domain.Products;

namespace SuperStorage.Application.Features.Categories;

public interface ICategoryRepository : IRepository<Category, Guid>
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid excludingId, CancellationToken cancellationToken = default);
}
