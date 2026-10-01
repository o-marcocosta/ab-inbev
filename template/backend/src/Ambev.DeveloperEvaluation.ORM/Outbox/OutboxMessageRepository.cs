using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly DefaultContext _context;

    public OutboxMessageRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<OutboxMessage>> LockPendingAsync(int batchSize, int maxAttempts,
        CancellationToken cancellationToken = default) =>
        await _context.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT * FROM "OutboxMessages"
                WHERE "ProcessedAt" IS NULL AND "Attempts" < {maxAttempts}
                ORDER BY "OccurredAt"
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);
}
