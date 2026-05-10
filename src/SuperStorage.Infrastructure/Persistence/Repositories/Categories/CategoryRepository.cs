using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Features.Categories;
using SuperStorage.Domain.Products;

namespace SuperStorage.Infrastructure.Persistence.Repositories.Categories;

internal sealed class CategoryRepository(WmsDbContext dbContext)
    : Repository<Category, Guid>(dbContext), ICategoryRepository
{
    public async Task<Category?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Category>()
            .SingleOrDefaultAsync(category => category.Name == name.Trim(), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Category>()
            .AnyAsync(category => category.Name == name.Trim(), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        Guid excludingId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext
            .Set<Category>()
            .AnyAsync(category => category.Id != excludingId && category.Name == name.Trim(), cancellationToken);
    }
}
