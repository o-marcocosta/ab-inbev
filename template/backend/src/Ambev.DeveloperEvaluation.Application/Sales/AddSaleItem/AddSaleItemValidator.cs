using Ambev.DeveloperEvaluation.Application.Sales.Common;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddSaleItem;

public sealed class AddSaleItemCommandValidator : AbstractValidator<AddSaleItemCommand>
{
    public AddSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        Include(new SaleItemDataValidator());
    }
}
