namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public interface ISaleDetails
{
    DateTime SaleDate { get; }
    Guid CustomerId { get; }
    string CustomerName { get; }
    Guid BranchId { get; }
    string BranchName { get; }
}
