using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public sealed record CancelSaleItemCommand(Guid SaleId, Guid ItemId) : IRequest<SaleResult>, ITransactionalRequest;
