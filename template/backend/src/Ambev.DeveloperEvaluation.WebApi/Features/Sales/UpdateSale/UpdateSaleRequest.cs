namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public sealed record UpdateSaleRequest
{
    public DateTime SaleDate { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
}
