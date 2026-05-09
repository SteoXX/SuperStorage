using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperStorage.Application.Abstractions.Persistence;
using SuperStorage.Contracts.Auth;
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

            services.ConfigureApplicationCookie(ConfigureApplicationCookie);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthPolicies.ProductsRead, policy =>
                    policy.RequireRole(
                        AuthRoles.Administrator,
                        AuthRoles.WarehouseManager,
                        AuthRoles.Operator,
                        AuthRoles.Viewer));

                options.AddPolicy(AuthPolicies.ProductsWrite, policy =>
                    policy.RequireRole(
                        AuthRoles.Administrator,
                        AuthRoles.WarehouseManager));

                options.AddPolicy(AuthPolicies.CategoriesRead, policy =>
                    policy.RequireRole(
                        AuthRoles.Administrator,
                        AuthRoles.WarehouseManager,
                        AuthRoles.Operator,
                        AuthRoles.Viewer));

                options.AddPolicy(AuthPolicies.CategoriesWrite, policy =>
                    policy.RequireRole(
                        AuthRoles.Administrator,
                        AuthRoles.WarehouseManager));

                options.AddPolicy(AuthPolicies.UsersManage, policy =>
                    policy.RequireRole(AuthRoles.Administrator));
            });

            services.AddScoped<IQueryDbContext>(provider => provider.GetRequiredService<WmsDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register repositories by convention
            RegisterRepositories(services);

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

    private static void ConfigureApplicationCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "__Host-SuperStorage.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Path = "/";
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";

        options.Events.OnRedirectToLogin = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    }

    private static bool IsApiRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api");
    }

    /// <summary>
    ///     Registers all repository implementations in the assembly by convention.
    ///     
    ///     Convention: For a repository implementation named 'ProductRepository', it should implement an interface named 'IProductRepository'.
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"));

        foreach (var implementationType in repositoryTypes)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

            if (interfaceType is not null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }
    }
}
