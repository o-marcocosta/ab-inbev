using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed record UpdateSaleCommand : ISaleDetails, IRequest<SaleResult>, ITransactionalRequest
{
    public Guid Id { get; init; }
    public DateTime SaleDate { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
}
