using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.AdjustSaleItemQuantity;

// A delta is commutative, so on a concurrency conflict it is re-applied on top of the newer state.
public class AdjustSaleItemQuantityHandler : IRequestHandler<AdjustSaleItemQuantityCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public AdjustSaleItemQuantityHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public Task<SaleResult> Handle(AdjustSaleItemQuantityCommand command, CancellationToken cancellationToken) =>
        ConcurrencyRetry.ExecuteAsync(async () =>
        {
            var sale = await _saleRepository.GetRequiredAsync(command.SaleId, cancellationToken);
            sale.EnsureHasItem(command.ItemId);

            sale.AdjustItemQuantity(command.ItemId, command.Delta);

            await _saleRepository.UpdateAsync(sale, cancellationToken);
            return _mapper.Map<SaleResult>(sale);
        });
}
