namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

public sealed record SaleItemResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    public decimal GrossAmount { get; init; }

    public decimal DiscountRate { get; init; }

    // Rounded per line. There is no per-unit discounted price: rounding per unit would not add up to the line total.
    public decimal DiscountAmount { get; init; }

    public decimal TotalAmount { get; init; }
    public bool IsCancelled { get; init; }
    public DateTime? CancelledAt { get; init; }
}
