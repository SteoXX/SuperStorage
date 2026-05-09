using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SuperStorage.Application;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            var assembly = typeof(ServiceCollectionExtensions).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                // Behavior pipeline
                // cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                // cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
                // cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

            // services.AddScoped<ICommandDispatcher, CommandDispatcher>();
            // services.AddScoped<IQueryDispatcher, QueryDispatcher>();

            return services;
        }
    }
}
