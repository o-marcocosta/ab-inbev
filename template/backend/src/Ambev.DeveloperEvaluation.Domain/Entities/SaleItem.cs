using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Policies;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

// Discount values are persisted as a snapshot so historical sales are not affected by future rule changes.
// Can only be changed through the Sale aggregate.
public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public ProductRef Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountRate { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public decimal GrossAmount => Quantity * UnitPrice;

    private SaleItem() { }

    internal SaleItem(Guid saleId, ProductRef product, int quantity, decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        if (decimal.Round(unitPrice, 2) != unitPrice)
            throw new DomainException("Unit price must have at most two decimal places.");

        Id = Guid.NewGuid();
        SaleId = saleId;
        Product = product ?? throw new DomainException("Product must be provided.");
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    internal void ChangeQuantity(int quantity)
    {
        EnsureNotCancelled();
        SetQuantity(quantity);
    }

    internal void Cancel(DateTime cancelledAt)
    {
        EnsureNotCancelled();
        IsCancelled = true;
        CancelledAt = cancelledAt;
    }

    private void SetQuantity(int quantity)
    {
        DiscountRate = QuantityDiscountPolicy.GetDiscountRate(quantity);
        Quantity = quantity;
        DiscountAmount = decimal.Round(GrossAmount * DiscountRate, 2, MidpointRounding.AwayFromZero);
        TotalAmount = GrossAmount - DiscountAmount;
    }

    private void EnsureNotCancelled()
    {
        if (IsCancelled)
            throw new DomainException("Sale item is already cancelled.");
    }
}
