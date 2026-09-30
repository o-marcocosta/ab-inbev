using Ambev.DeveloperEvaluation.Application.Common.Messaging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public abstract record SaleSnapshotIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredAt { get; init; }
    public Guid SaleId { get; init; }
    public long SaleNumber { get; init; }
    public DateTime SaleDate { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<SaleEventItem> Items { get; init; } = [];
}
