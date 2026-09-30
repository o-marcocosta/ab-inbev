using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Common.Persistence;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest)
            return next();

        return _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var response = await next();

            // Handlers add outbox messages after saving the sale (the sale number only exists after the
            // insert), so they are saved here, before the commit.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return response;
        }, cancellationToken);
    }
}
