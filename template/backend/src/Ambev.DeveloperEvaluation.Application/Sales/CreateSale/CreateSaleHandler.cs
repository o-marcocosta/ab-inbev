using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

// Builds the aggregate through its own operations, so lines for the same product are consolidated
// and discounts are calculated by the domain.
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = Sale.Create(
            command.SaleDate,
            new CustomerRef(command.CustomerId, command.CustomerName),
            new BranchRef(command.BranchId, command.BranchName));

        foreach (var item in command.Items)
            sale.AddItem(new ProductRef(item.ProductId, item.ProductName), item.Quantity, item.UnitPrice);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(created);
    }
}
