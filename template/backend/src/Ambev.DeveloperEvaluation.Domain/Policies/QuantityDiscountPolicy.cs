using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Policies;

// The challenge statement is ambiguous for exactly 4 items ("above 4" vs "4+ items").
// Adopted interpretation, consistent with "below 4 items cannot have a discount":
// 1-3 items: 0%, 4-9 items: 10%, 10-20 items: 20%, above 20: not allowed.
public static class QuantityDiscountPolicy
{
    public const int MaxQuantityPerProduct = 20;

    private const int TenPercentThreshold = 4;
    private const int TwentyPercentThreshold = 10;

    public static decimal GetDiscountRate(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (quantity > MaxQuantityPerProduct)
            throw new DomainException($"It is not possible to sell more than {MaxQuantityPerProduct} identical items.");

        if (quantity >= TwentyPercentThreshold)
            return 0.20m;

        if (quantity >= TenPercentThreshold)
            return 0.10m;

        return 0m;
    }
}
