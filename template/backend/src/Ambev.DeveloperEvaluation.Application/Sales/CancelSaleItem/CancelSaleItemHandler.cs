using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IOutbox _outbox;
    private readonly IMapper _mapper;

    public CancelSaleItemHandler(ISaleRepository saleRepository, IOutbox outbox, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _outbox = outbox;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetRequiredAsync(command.SaleId, cancellationToken);
        sale.EnsureHasItem(command.ItemId);

        sale.CancelItem(command.ItemId);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        _outbox.Add(SaleIntegrationEvents.ItemCancelled(sale, sale.Items.Single(i => i.Id == command.ItemId)));

        return _mapper.Map<SaleResult>(sale);
    }
}
