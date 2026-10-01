using Ambev.DeveloperEvaluation.Domain.Common.Querying;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed class ListSalesQueryValidator : AbstractValidator<ListSalesQuery>
{
    public ListSalesQueryValidator()
    {
        RuleFor(x => x.Options.Page).GreaterThanOrEqualTo(1).OverridePropertyName("_page");
        RuleFor(x => x.Options.Size).InclusiveBetween(1, QueryOptions.MaxSize).OverridePropertyName("_size");
    }
}
