using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

// Turns a missing sale or item into a "not found" error instead of a business rule violation.
internal static class SaleLookupExtensions
{
    public static async Task<Sale> GetRequiredAsync(this ISaleRepository repository, Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken)
        ?? throw new KeyNotFoundException($"Sale with ID {id} not found");

    public static void EnsureHasItem(this Sale sale, Guid itemId)
    {
        if (sale.Items.All(i => i.Id != itemId))
            throw new KeyNotFoundException($"Item with ID {itemId} not found in sale {sale.Id}");
    }
}
