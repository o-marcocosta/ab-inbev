namespace Ambev.DeveloperEvaluation.Messaging.Polling;

public sealed record PollingSchedule<TJob>(TimeSpan Interval) where TJob : IPollingJob;
