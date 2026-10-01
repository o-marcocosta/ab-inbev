namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> LockPendingAsync(int batchSize, int maxAttempts, CancellationToken cancellationToken = default);
}
