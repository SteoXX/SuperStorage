using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Domain.Common;

namespace SuperStorage.Infrastructure.Persistence.Repositories;

internal abstract class Repository<TAggregate, TId>(WmsDbContext dbContext)
    : IRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    public async Task<TAggregate?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TAggregate>().FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TAggregate>().AddAsync(aggregate, cancellationToken);
    }

    public void Remove(TAggregate aggregate)
    {
        dbContext.Set<TAggregate>().Remove(aggregate);
    }
}
