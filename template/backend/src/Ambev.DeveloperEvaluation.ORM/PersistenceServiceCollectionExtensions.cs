using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.ORM;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultContext(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<DefaultContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
            )
        );

        services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // Web API.
    public static IServiceCollection AddPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDefaultContext(connectionString);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IOutbox, OutboxStore>();

        return services;
    }

    // Outbox worker.
    public static IServiceCollection AddOutboxRelayPersistence(this IServiceCollection services, string? connectionString)
    {
        services.AddDefaultContext(connectionString);

        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        return services;
    }
}
