using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Messaging.Polling;

public sealed class PollingService<TJob> : BackgroundService where TJob : IPollingJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval;
    private readonly ILogger<PollingService<TJob>> _logger;

    public PollingService(IServiceScopeFactory scopeFactory, PollingSchedule<TJob> schedule, ILogger<PollingService<TJob>> logger)
    {
        _scopeFactory = scopeFactory;
        _interval = schedule.Interval;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        do
        {
            await RunUntilIdleAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunUntilIdleAsync(CancellationToken stoppingToken)
    {
        bool morePending;
        do
        {
            morePending = await RunOnceAsync(stoppingToken);
        }
        while (morePending);
    }

    private async Task<bool> RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<TJob>().ExecuteAsync(stoppingToken);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            // A failed run must not stop the service; the next tick tries again.
            _logger.LogError(ex, "{Job} failed", typeof(TJob).Name);
            return false;
        }
    }
}
