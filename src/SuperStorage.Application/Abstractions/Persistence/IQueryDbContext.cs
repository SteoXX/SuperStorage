namespace SuperStorage.Application.Abstractions.Persistence;

public interface IQueryDbContext
{
    IQueryable<TEntity> Query<TEntity>()
        where TEntity : class;

    Task<List<TEntity>> ToListAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);

    Task<TEntity?> SingleOrDefaultAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default);
}
