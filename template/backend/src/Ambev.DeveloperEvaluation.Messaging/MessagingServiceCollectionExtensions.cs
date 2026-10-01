using Ambev.DeveloperEvaluation.Messaging.Outbox;
using Ambev.DeveloperEvaluation.Messaging.Polling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Auditing.Messages;
using Rebus.Config;

namespace Ambev.DeveloperEvaluation.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));

        var conn = GetRabbitMqConnectionString(configuration);
        services.AddRebus((configure, provider) => ConfigureRebus(configure, GetOptions(provider), conn));

        services.AddPollingJob<OutboxDispatchJob>(provider => GetOptions(provider).PollingInterval);

        return services;
    }

    // One-way: this service only publishes, it has no input queue and consumes nothing.
    private static RebusConfigurer ConfigureRebus(RebusConfigurer configure, MessagingOptions options, string connectionString) =>
        configure
            .Transport(t => t.UseRabbitMqAsOneWayClient(connectionString))
            .Options(o => o.EnableMessageAuditing(options.AuditQueue));

    private static MessagingOptions GetOptions(IServiceProvider provider) =>
        provider.GetRequiredService<IOptions<MessagingOptions>>().Value;

    private static string GetRabbitMqConnectionString(IConfiguration configuration) =>
        configuration.GetConnectionString("RabbitMq")
            ?? throw new InvalidOperationException("Connection string 'RabbitMq' is not configured.");
}
