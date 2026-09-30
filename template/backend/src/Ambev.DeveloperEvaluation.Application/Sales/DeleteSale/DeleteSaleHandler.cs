using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, Unit>
{
    private readonly ISaleRepository _saleRepository;

    public DeleteSaleHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Unit> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        if (!await _saleRepository.DeleteAsync(command.Id, cancellationToken))
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        return Unit.Value;
    }
}
