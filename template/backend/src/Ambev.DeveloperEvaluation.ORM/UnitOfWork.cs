using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Ambev.DeveloperEvaluation.ORM;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DefaultContext _context;

    public UnitOfWork(DefaultContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        // READ COMMITTED on purpose: ConcurrencyRetry reloads the aggregate inside this transaction and must
        // see the version committed by the competing request. With a snapshot isolation level every retry
        // would read the same stale version. A failed SaveChanges rolls back to a savepoint created by EF,
        // so the transaction stays usable for the retry.
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var result = await operation();

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
