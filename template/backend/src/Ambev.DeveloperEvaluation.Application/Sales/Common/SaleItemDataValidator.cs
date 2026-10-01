using Ambev.DeveloperEvaluation.Domain.Policies;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed class SaleItemDataValidator : AbstractValidator<ISaleItemData>
{
    public SaleItemDataValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Quantity).InclusiveBetween(1, QuantityDiscountPolicy.MaxQuantityPerProduct);
        RuleFor(x => x.UnitPrice).GreaterThan(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
    }
}
