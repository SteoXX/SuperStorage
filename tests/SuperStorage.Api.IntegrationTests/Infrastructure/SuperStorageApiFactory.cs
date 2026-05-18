using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperStorage.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SuperStorage.Api.IntegrationTests.Infrastructure;

public sealed class SuperStorageApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ConnectionStringConfigurationKey = "ConnectionStrings__WmsDatabase";
    private const string AdminEmailConfigurationKey = "IdentitySeed__AdminEmail";
    private const string AdminPasswordConfigurationKey = "IdentitySeed__AdminPassword";
    private const string AdminDisplayNameConfigurationKey = "IdentitySeed__AdminDisplayName";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("SuperStorageTests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string AdminEmail { get; } = "admin.integration@superstorage.test";

    public string AdminPassword { get; } = "Admin12345!";

    public static async Task<SuperStorageApiFactory> CreateAsync()
    {
        var factory = new SuperStorageApiFactory();
        await factory.InitializeAsync();
        return factory;
    }

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        Environment.SetEnvironmentVariable(
            ConnectionStringConfigurationKey,
            _database.GetConnectionString());
        Environment.SetEnvironmentVariable(AdminEmailConfigurationKey, AdminEmail);
        Environment.SetEnvironmentVariable(AdminPasswordConfigurationKey, AdminPassword);
        Environment.SetEnvironmentVariable(AdminDisplayNameConfigurationKey, "Integration Test Admin");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();

        Environment.SetEnvironmentVariable(ConnectionStringConfigurationKey, null);
        Environment.SetEnvironmentVariable(AdminEmailConfigurationKey, null);
        Environment.SetEnvironmentVariable(AdminPasswordConfigurationKey, null);
        Environment.SetEnvironmentVariable(AdminDisplayNameConfigurationKey, null);
    }

    public HttpClient CreateHttpsClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:WmsDatabase"] = _database.GetConnectionString(),
                ["IdentitySeed:AdminEmail"] = AdminEmail,
                ["IdentitySeed:AdminPassword"] = AdminPassword,
                ["IdentitySeed:AdminDisplayName"] = "Integration Test Admin"
            });
        });

        builder.ConfigureServices(services =>
        {
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<SuperStorageDbContext>();
            dbContext.Database.Migrate();
        });
    }
}
