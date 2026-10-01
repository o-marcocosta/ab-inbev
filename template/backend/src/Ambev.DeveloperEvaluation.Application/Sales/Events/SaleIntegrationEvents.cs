using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public static class SaleIntegrationEvents
{
    public static SaleCreatedIntegrationEvent Created(Sale sale) => Snapshot<SaleCreatedIntegrationEvent>(sale);

    public static SaleModifiedIntegrationEvent Modified(Sale sale) => Snapshot<SaleModifiedIntegrationEvent>(sale);

    public static SaleCancelledIntegrationEvent Cancelled(Sale sale) => new()
    {
        EventId = Guid.NewGuid(),
        OccurredAt = DateTime.UtcNow,
        SaleId = sale.Id,
        SaleNumber = sale.SaleNumber,
        CancelledAt = sale.CancelledAt ?? DateTime.UtcNow,
        TotalAmount = sale.TotalAmount
    };

    public static SaleItemCancelledIntegrationEvent ItemCancelled(Sale sale, SaleItem item) => new()
    {
        EventId = Guid.NewGuid(),
        OccurredAt = DateTime.UtcNow,
        SaleId = sale.Id,
        SaleNumber = sale.SaleNumber,
        ItemId = item.Id,
        ProductId = item.Product.Id,
        ProductName = item.Product.Name,
        Quantity = item.Quantity,
        CancelledAt = item.CancelledAt ?? DateTime.UtcNow,
        SaleTotalAmount = sale.TotalAmount
    };

    private static TEvent Snapshot<TEvent>(Sale sale) where TEvent : SaleSnapshotIntegrationEvent, new() => new()
    {
        EventId = Guid.NewGuid(),
        OccurredAt = DateTime.UtcNow,
        SaleId = sale.Id,
        SaleNumber = sale.SaleNumber,
        SaleDate = sale.SaleDate,
        CustomerId = sale.Customer.Id,
        CustomerName = sale.Customer.Name,
        BranchId = sale.Branch.Id,
        BranchName = sale.Branch.Name,
        TotalAmount = sale.TotalAmount,
        Items = sale.Items.Select(i => new SaleEventItem
        {
            ItemId = i.Id,
            ProductId = i.Product.Id,
            ProductName = i.Product.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountRate = i.DiscountRate,
            DiscountAmount = i.DiscountAmount,
            TotalAmount = i.TotalAmount,
            IsCancelled = i.IsCancelled
        }).ToList()
    };
}
