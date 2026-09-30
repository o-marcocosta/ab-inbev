using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

// Re-runs a load-change-save operation when the aggregate was changed concurrently.
// Only safe for commutative operations (adding an item, adjusting a quantity by a delta): re-applying
// them on top of the newer state keeps both changes. Operations that set an absolute state must not be
// retried; their conflict is returned to the client instead.
internal static class ConcurrencyRetry
{
    public const int MaxAttempts = 3;

    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                // The repository discarded the stale state; the next attempt reloads the current version.
            }
        }
    }
}
