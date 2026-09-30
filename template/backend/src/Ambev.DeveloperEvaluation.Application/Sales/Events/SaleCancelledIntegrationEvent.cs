using Ambev.DeveloperEvaluation.Application.Common.Messaging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed record SaleCancelledIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public Guid SaleId { get; init; }
    public long SaleNumber { get; init; }
    public DateTime CancelledAt { get; init; }
    public decimal TotalAmount { get; init; }
}
