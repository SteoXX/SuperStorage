using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Domain.Products;
using SuperStorage.Infrastructure.Persistence.Identity;

namespace SuperStorage.Infrastructure.Persistence;

public sealed class WmsDbContext(DbContextOptions<WmsDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IQueryDbContext
{
    // ==== Schemas ====
    public const string WmsSchema = "Wms";
    public const string IdentitySchema = "Identity";
    public const string LoggingSchema = "Logging";

    // ==== DbSets ====
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(WmsSchema);

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(WmsDbContext).Assembly);
    }


    // ==== IQueryDbContext implementation ====

    /// <summary>
    ///     Returns an <see cref="IQueryable{TEntity}"/> for the specified entity type.
    /// 
    ///     The returned queryable is configured to not track changes to the entities, which can improve performance for read-only operations.
    /// </summary>
    public IQueryable<TEntity> Query<TEntity>()
        where TEntity : class
    {
        return Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    ///    Executes the provided query and returns the results as a list.
    /// </summary>
    public async Task<List<TEntity>> ToListAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    ///     Executes the provided query and returns a single entity that matches the criteria, or null if no such entity exists.
    /// </summary>
    public async Task<TEntity?> SingleOrDefaultAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    ///     Executes the provided query and returns the number of entities that match the criteria.
    /// </summary>
    public async Task<int> CountAsync<TEntity>(
        IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
    {
        return await query.CountAsync(cancellationToken);
    }
}
