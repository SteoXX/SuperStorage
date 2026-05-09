namespace SuperStorage.Application.Abstractions.Persistence;

public interface IQueryDbContext
{
    IQueryable<TEntity> Query<TEntity>()
        where TEntity : class;
}
