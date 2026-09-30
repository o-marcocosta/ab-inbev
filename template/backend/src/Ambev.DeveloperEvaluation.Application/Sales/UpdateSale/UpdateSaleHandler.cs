using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

// Sets absolute values, so a concurrency conflict is not retried: it is surfaced to the client,
// who must reload before deciding again.
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetRequiredAsync(command.Id, cancellationToken);

        sale.UpdateDetails(
            command.SaleDate,
            new CustomerRef(command.CustomerId, command.CustomerName),
            new BranchRef(command.BranchId, command.BranchName));

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        return _mapper.Map<SaleResult>(sale);
    }
}
