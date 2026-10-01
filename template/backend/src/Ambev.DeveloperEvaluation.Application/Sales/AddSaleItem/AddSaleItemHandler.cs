using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddSaleItem;

// Adding is a relative change, so a concurrency conflict is retried instead of surfaced.
public class AddSaleItemHandler : IRequestHandler<AddSaleItemCommand, SaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IOutbox _outbox;
    private readonly IMapper _mapper;

    public AddSaleItemHandler(ISaleRepository saleRepository, IOutbox outbox, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _outbox = outbox;
        _mapper = mapper;
    }

    public Task<SaleResult> Handle(AddSaleItemCommand command, CancellationToken cancellationToken) =>
        ConcurrencyRetry.ExecuteAsync(async () =>
        {
            var sale = await _saleRepository.GetRequiredAsync(command.SaleId, cancellationToken);

            sale.AddItem(new ProductRef(command.ProductId, command.ProductName), command.Quantity, command.UnitPrice);

            await _saleRepository.UpdateAsync(sale, cancellationToken);
            _outbox.Add(SaleIntegrationEvents.Modified(sale));

            return _mapper.Map<SaleResult>(sale);
        });
}
