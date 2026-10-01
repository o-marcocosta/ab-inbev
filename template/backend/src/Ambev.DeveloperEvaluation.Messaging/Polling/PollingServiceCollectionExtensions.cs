using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Messaging.Polling;

public static class PollingServiceCollectionExtensions
{
    public static IServiceCollection AddPollingJob<TJob>(this IServiceCollection services, Func<IServiceProvider, TimeSpan> interval)
        where TJob : class, IPollingJob
    {
        services.AddScoped<TJob>();
        services.AddSingleton(provider => new PollingSchedule<TJob>(interval(provider)));
        services.AddHostedService<PollingService<TJob>>();

        return services;
    }
}
