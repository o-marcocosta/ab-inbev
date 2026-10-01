using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.AdjustSaleItemQuantity;

// Whether the resulting quantity is valid depends on the current state, so that check belongs to the aggregate.
public sealed class AdjustSaleItemQuantityCommandValidator : AbstractValidator<AdjustSaleItemQuantityCommand>
{
    public AdjustSaleItemQuantityCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Delta)
            .NotEqual(0)
            .InclusiveBetween(-QuantityDiscountPolicy.MaxQuantityPerProduct, QuantityDiscountPolicy.MaxQuantityPerProduct);
    }
}
