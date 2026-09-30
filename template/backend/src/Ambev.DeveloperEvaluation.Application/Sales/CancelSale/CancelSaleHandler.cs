using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IOutbox _outbox;
    private readonly IMapper _mapper;

    public CancelSaleHandler(ISaleRepository saleRepository, IOutbox outbox, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _outbox = outbox;
        _mapper = mapper;
    }

    public async Task<SaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetRequiredAsync(command.Id, cancellationToken);

        sale.Cancel();

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        _outbox.Add(SaleIntegrationEvents.Cancelled(sale));

        return _mapper.Map<SaleResult>(sale);
    }
}
