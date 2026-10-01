using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.AdjustSaleItemQuantity;

// Relative change: the client does not need the current quantity, so a stale copy (another tab, a cache)
// cannot overwrite a newer one. Not idempotent: sending the same command twice applies it twice.
public sealed record AdjustSaleItemQuantityCommand : IRequest<SaleResult>, ITransactionalRequest
{
    public Guid SaleId { get; init; }
    public Guid ItemId { get; init; }
    public int Delta { get; init; }
}
