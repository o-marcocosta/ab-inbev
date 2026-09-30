namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed record SaleItemResult
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal GrossAmount { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public bool IsCancelled { get; init; }
    public DateTime? CancelledAt { get; init; }
}
