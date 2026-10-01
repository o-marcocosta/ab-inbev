using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class TransactionBehaviorTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly List<string> _steps = [];

    public TransactionBehaviorTests()
    {
        // Runs the operation as the real unit of work would, recording where the transaction starts and ends.
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<Task<string>>>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                _steps.Add("begin");
                var result = await call.Arg<Func<Task<string>>>()();
                _steps.Add("commit");
                return result;
            });

        _unitOfWork.When(u => u.SaveChangesAsync(Arg.Any<CancellationToken>())).Do(_ => _steps.Add("save outbox"));
    }

    [Fact(DisplayName = "Messages added by the handler should be saved in the same transaction, before the commit")]
    public async Task Given_TransactionalRequest_When_Handled_Then_ShouldSaveBeforeCommit()
    {
        var response = await Behavior<TransactionalRequest>().Handle(new TransactionalRequest(), Handler, CancellationToken.None);

        response.Should().Be("handled");
        _steps.Should().Equal("begin", "handler", "save outbox", "commit");
    }

    [Fact(DisplayName = "Requests that are not transactional should skip the transaction")]
    public async Task Given_PlainRequest_When_Handled_Then_ShouldNotOpenTransaction()
    {
        var response = await Behavior<PlainRequest>().Handle(new PlainRequest(), Handler, CancellationToken.None);

        response.Should().Be("handled");
        _steps.Should().Equal("handler");
    }

    [Fact(DisplayName = "A failing handler should not save nor commit")]
    public async Task Given_TransactionalRequest_When_HandlerFails_Then_ShouldNotCommit()
    {
        var act = () => Behavior<TransactionalRequest>().Handle(
            new TransactionalRequest(), () => throw new InvalidOperationException("boom"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _steps.Should().Equal("begin");
    }

    private TransactionBehavior<TRequest, string> Behavior<TRequest>() where TRequest : IRequest<string> =>
        new(_unitOfWork);

    private Task<string> Handler()
    {
        _steps.Add("handler");
        return Task.FromResult("handled");
    }

    public sealed record TransactionalRequest : IRequest<string>, ITransactionalRequest;

    public sealed record PlainRequest : IRequest<string>;
}
