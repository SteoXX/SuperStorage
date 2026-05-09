using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Infrastructure.Persistence;
using SuperStorage.Infrastructure.Persistence.Identity;

namespace SuperStorage.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("WmsDatabase");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'WmsDatabase' was not found.");
            }

            services.AddDbContext<WmsDbContext>(options =>
            {
                options
                    .UseNpgsql(
                        connectionString,
                        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            WmsDbContext.WmsSchema));
            });

            // Identity configuration
            services
                .AddIdentityCore<ApplicationUser>(ConfigureIdentityOptions)
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<WmsDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            // Authentication and Authorization configuration
            services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();

            services.AddAuthorization();
            services.AddScoped<IQueryDbContext>(provider => provider.GetRequiredService<WmsDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    }
}
