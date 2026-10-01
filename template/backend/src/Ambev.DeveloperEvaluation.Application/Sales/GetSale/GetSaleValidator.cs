using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public sealed class GetSaleQueryValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
