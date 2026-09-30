using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.ORM.Querying.FieldMaps;

// Fields of a sale that a client may filter and sort by.
public static class SaleFieldMap
{
    public static readonly FieldMap<Sale> Fields = new FieldMap<Sale>()
        .Add("saleNumber", s => s.SaleNumber)
        .Add("saleDate", s => s.SaleDate)
        .Add("customerId", s => s.Customer.Id)
        .Add("customerName", s => s.Customer.Name)
        .Add("branchId", s => s.Branch.Id)
        .Add("branchName", s => s.Branch.Name)
        .Add("totalAmount", s => s.TotalAmount)
        .Add("isCancelled", s => s.IsCancelled)
        .Add("createdAt", s => s.CreatedAt);
}
