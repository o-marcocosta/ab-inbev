using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Ambev.DeveloperEvaluation.Messaging.Polling;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Messages;

namespace Ambev.DeveloperEvaluation.Messaging.Outbox;

public sealed class OutboxDispatchJob : IPollingJob
{
    private readonly IOutboxMessageRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBus _bus;
    private readonly MessagingOptions _options;
    private readonly ILogger<OutboxDispatchJob> _logger;

    public OutboxDispatchJob(
        IOutboxMessageRepository repository,
        IUnitOfWork unitOfWork,
        IBus bus,
        IOptions<MessagingOptions> options,
        ILogger<OutboxDispatchJob> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _bus = bus;
        _options = options.Value;
        _logger = logger;
    }

    // The messages stay locked while they are published, and their new state is saved in the same transaction.
    public Task<bool> ExecuteAsync(CancellationToken cancellationToken) =>
        _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var messages = await _repository.LockPendingAsync(_options.BatchSize, _options.MaxAttempts, cancellationToken);
            var allDispatched = await ProcessAsync(messages);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // check if there are more messages to process, so the job can run again without waiting for the next tick.
            var hasMorePending = allDispatched && messages.Count == _options.BatchSize;
            return hasMorePending;
        }, cancellationToken);


    private async Task<bool> ProcessAsync(IReadOnlyList<OutboxMessage> messages)
    {
        foreach (var message in messages)
        {
            // Stops at the first failure, to keep the order of messages.
            if (!await DispatchAsync(message)) return false;
        }

        return true;
    }

    private async Task<bool> DispatchAsync(OutboxMessage message) =>
        TryRead(message, out var payload) && await TryPublishAsync(message, payload);

    private bool TryRead(OutboxMessage message, out object payload)
    {
        try
        {
            payload = message.ToMessage();
            return true;
        }
        catch (Exception ex)
        {
            // Will never succeed, so it counts towards MaxAttempts until it is set aside.
            message.MarkFailed(ex.Message);
            _logger.LogError(ex, "Outbox message {MessageId} ({MessageType}) cannot be read, attempt {Attempt}",
                message.Id, message.Type, message.Attempts);
            payload = null!;
            return false;
        }
    }

    private async Task<bool> TryPublishAsync(OutboxMessage message, object payload)
    {
        try
        {
            // The message id is the event id, so consumers can discard redeliveries.
            var headers = new Dictionary<string, string> { [Headers.MessageId] = message.Id.ToString() };
            await _bus.Publish(payload, headers);

            message.MarkProcessed(DateTime.UtcNow);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish outbox message {MessageId} ({MessageType})", message.Id, message.Type);
            return false;
        }
    }
}
