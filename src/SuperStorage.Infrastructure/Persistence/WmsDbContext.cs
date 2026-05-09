using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SuperStorage.Infrastructure.Persistence.Identity;

namespace SuperStorage.Infrastructure.Persistence;

public sealed class WmsDbContext(DbContextOptions<WmsDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public const string WmsSchema = "Wms";
    public const string IdentitySchema = "Identity";
    public const string LoggingSchema = "Logging";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(WmsSchema);

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(WmsDbContext).Assembly);
    }
}
