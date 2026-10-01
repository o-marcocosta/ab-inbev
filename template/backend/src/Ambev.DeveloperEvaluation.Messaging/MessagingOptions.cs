namespace Ambev.DeveloperEvaluation.Messaging;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    // Receives a copy of every published message, so they can be inspected.
    public string AuditQueue { get; set; } = "developer-evaluation.audit";

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);
    public int BatchSize { get; set; } = 50;

    // Messages that could not be read this many times are no longer picked up and stay in the table for
    // inspection. Publish failures (broker unavailable) are retried without counting.
    public int MaxAttempts { get; set; } = 10;
}
