using SuperStorage.Domain.Common;

namespace SuperStorage.Application.Abstractions.Persistence;

public interface IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default);
}
