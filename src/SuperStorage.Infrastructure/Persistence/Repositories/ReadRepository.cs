using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Domain.Common;

namespace SuperStorage.Infrastructure.Persistence.Repositories;

internal abstract class ReadRepository<TEntity, TId>(IReadDbContext dbContext)
    : IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected IReadDbContext ReadDbContext { get; } = dbContext;

    protected IQueryable<TEntity> Query()
    {
        return ReadDbContext.Query<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .SingleOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);
    }
}
