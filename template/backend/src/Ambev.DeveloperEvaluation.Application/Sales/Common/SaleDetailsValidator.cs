using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public sealed class SaleDetailsValidator : AbstractValidator<ISaleDetails>
{
    public SaleDetailsValidator()
    {
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(150);
    }
}
