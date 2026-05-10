using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SuperStorage.Infrastructure.Persistence.Design;

/// <summary>
///     Factory for creating instances of WmsDbContext at design time, such as during migrations.
///     It reads the database connection string from the environment variable 'SUPERSTORAGE_CONNECTION_STRING'.
///     You can set the environment variable using:
/// 
///     ```
///     export SUPERSTORAGE_CONNECTION_STRING="Host=localhost;Port=5432;Database=x;Username=x;Password=x"
///     ```
public sealed class SuperStorageDbContextFactory
    : IDesignTimeDbContextFactory<SuperStorageDbContext>
{
    public SuperStorageDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SUPERSTORAGE_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Environment variable 'SUPERSTORAGE_CONNECTION_STRING' not found.");

        var options = new DbContextOptionsBuilder<SuperStorageDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new SuperStorageDbContext(options);
    }
}
