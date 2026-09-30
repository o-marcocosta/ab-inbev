using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

// Permanent removal. To keep the sale for history, use CancelSaleCommand instead.
public sealed record DeleteSaleCommand(Guid Id) : IRequest<Unit>;
