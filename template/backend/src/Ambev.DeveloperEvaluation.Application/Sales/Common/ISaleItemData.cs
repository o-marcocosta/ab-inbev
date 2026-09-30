namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public interface ISaleItemData
{
    Guid ProductId { get; }
    string ProductName { get; }
    int Quantity { get; }
    decimal UnitPrice { get; }
}
