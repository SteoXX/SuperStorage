using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Domain.Common;

namespace SuperStorage.Infrastructure.Persistence.Repositories;

internal abstract class Repository<TAggregate, TId>(SuperStorageDbContext dbContext)
    : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected SuperStorageDbContext DbContext { get; } = dbContext;

    public async Task<TAggregate?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<TAggregate>().FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TAggregate>().AddAsync(aggregate, cancellationToken);
    }

    public void Remove(TAggregate aggregate)
    {
        DbContext.Set<TAggregate>().Remove(aggregate);
    }
}
