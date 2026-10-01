using Ambev.DeveloperEvaluation.Application.Common.Messaging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed record SaleItemCancelledIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public Guid SaleId { get; init; }
    public long SaleNumber { get; init; }
    public Guid ItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public DateTime CancelledAt { get; init; }
    public decimal SaleTotalAmount { get; init; }
}
