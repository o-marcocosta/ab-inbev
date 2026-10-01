using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

// Always loads and saves the aggregate as a whole (sale + items).
public interface ISaleRepository
{
    // Returns the sale with its database-generated SaleNumber.
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Read-only: the returned sales are not tracked. Throws InvalidQueryException for unsupported fields or values.
    Task<PagedResult<Sale>> ListAsync(QueryOptions options, CancellationToken cancellationToken = default);

    // Throws ConcurrencyConflictException when the sale was changed since it was loaded. Implementations must
    // discard the stale state so that a subsequent GetByIdAsync reads the current version.
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
