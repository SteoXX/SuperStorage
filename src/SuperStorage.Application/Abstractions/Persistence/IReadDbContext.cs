namespace SuperStorage.Application.Abstractions.Persistence;

public interface IReadDbContext
{
    IQueryable<TEntity> Query<TEntity>()
        where TEntity : class;
}
