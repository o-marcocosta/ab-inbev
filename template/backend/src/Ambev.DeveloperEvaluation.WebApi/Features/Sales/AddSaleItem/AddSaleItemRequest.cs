namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddSaleItem;

public sealed record AddSaleItemRequest
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
