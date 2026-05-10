using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperStorage.Domain.Products;
using SuperStorage.Infrastructure.Persistence.Identity;

namespace SuperStorage.Infrastructure.Persistence;

public sealed class WmsDbContext(DbContextOptions<WmsDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    // ==== Schemas ====
    public const string WmsSchema = "Wms";
    public const string IdentitySchema = "Identity";
    public const string LoggingSchema = "Logging";

    // ==== DbSets ====
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(WmsSchema);

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(WmsDbContext).Assembly);
    }
}
