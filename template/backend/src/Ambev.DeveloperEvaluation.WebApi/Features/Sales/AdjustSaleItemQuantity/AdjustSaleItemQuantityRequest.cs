namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.AdjustSaleItemQuantity;

public sealed record AdjustSaleItemQuantityRequest
{
    public int Delta { get; init; }
}
