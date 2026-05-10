using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Abstractions.Persistence;

namespace SuperStorage.Infrastructure.Persistence;

internal sealed class SuperStorageReadDbContext(WmsDbContext dbContext) : IReadDbContext
{
    public IQueryable<TEntity> Query<TEntity>()
        where TEntity : class
    {
        return dbContext
            .Set<TEntity>()
            .AsNoTracking();
    }
}
