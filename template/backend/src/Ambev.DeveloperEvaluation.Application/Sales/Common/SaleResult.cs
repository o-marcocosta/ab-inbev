namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed record SaleResult
{
    public Guid Id { get; init; }
    public long SaleNumber { get; init; }
    public DateTime SaleDate { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public decimal GrossAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public bool IsCancelled { get; init; }
    public DateTime? CancelledAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<SaleItemResult> Items { get; init; } = [];
}
