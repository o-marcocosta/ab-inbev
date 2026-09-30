namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public sealed record SaleEventItem
{
    public Guid ItemId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountRate { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public bool IsCancelled { get; init; }
}
