using Ambev.DeveloperEvaluation.Application.Common.Persistence;
using Ambev.DeveloperEvaluation.Application.Sales.Events;
using Ambev.DeveloperEvaluation.Messaging;
using Ambev.DeveloperEvaluation.Messaging.Outbox;
using Ambev.DeveloperEvaluation.ORM.Outbox;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using Rebus.Bus;
using Rebus.Messages;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Messaging;

public class OutboxDispatchJobTests
{
    private const int BatchSize = 3;
    private const int MaxAttempts = 5;

    private readonly IOutboxMessageRepository _repository = Substitute.For<IOutboxMessageRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IBus _bus = Substitute.For<IBus>();
    private readonly List<Guid> _published = [];

    public OutboxDispatchJobTests()
    {
        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<Task<bool>>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<Task<bool>>>()());

        // Records only successful publishes, in the order they happened.
        _bus.Publish(Arg.Any<object>(), Arg.Any<IDictionary<string, string>>())
            .Returns(call =>
            {
                _published.Add(((SaleCancelledIntegrationEvent)call.Arg<object>()).EventId);
                return Task.CompletedTask;
            });
    }

    [Fact(DisplayName = "Pending messages should be published in order and marked as processed")]
    public async Task Given_PendingMessages_When_Executed_Then_ShouldPublishInOrder()
    {
        var messages = Pending(2);

        var morePending = await Job().ExecuteAsync(CancellationToken.None);

        _published.Should().Equal(messages.Select(m => m.Id));
        messages.Should().OnlyContain(m => m.ProcessedAt != null && m.Attempts == 0);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        morePending.Should().BeFalse();
    }

    [Fact(DisplayName = "The message id header should be the event id, so consumers can discard redeliveries")]
    public async Task Given_PendingMessage_When_Published_Then_ShouldUseEventIdAsMessageId()
    {
        var message = Pending(1).Single();

        await Job().ExecuteAsync(CancellationToken.None);

        await _bus.Received(1).Publish(
            Arg.Any<object>(),
            Arg.Is<IDictionary<string, string>>(h => h[Headers.MessageId] == message.Id.ToString()));
    }

    [Fact(DisplayName = "A full batch published without failures should report more pending work")]
    public async Task Given_FullBatch_When_AllPublished_Then_ShouldReportMorePending()
    {
        Pending(BatchSize);

        var morePending = await Job().ExecuteAsync(CancellationToken.None);

        morePending.Should().BeTrue();
    }

    [Fact(DisplayName = "A publish failure should stop the batch without counting an attempt")]
    public async Task Given_PublishFailure_When_Executed_Then_ShouldStopAndKeepMessagesPending()
    {
        var messages = Pending(BatchSize);
        _bus.Configure()
            .Publish(Arg.Is<object>(e => ((SaleCancelledIntegrationEvent)e).EventId == messages[1].Id), Arg.Any<IDictionary<string, string>>())
            .ThrowsAsync(new InvalidOperationException("broker unavailable"));

        var morePending = await Job().ExecuteAsync(CancellationToken.None);

        _published.Should().Equal(messages[0].Id);
        messages[0].ProcessedAt.Should().NotBeNull();
        messages.Skip(1).Should().OnlyContain(m => m.ProcessedAt == null && m.Attempts == 0);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        morePending.Should().BeFalse();
    }

    [Fact(DisplayName = "An unreadable message should count an attempt and hold back the ones after it")]
    public async Task Given_UnreadableMessage_When_Executed_Then_ShouldCountAttemptAndStop()
    {
        var messages = Pending(2);
        Corrupt(messages[0]);

        var morePending = await Job().ExecuteAsync(CancellationToken.None);

        _published.Should().BeEmpty();
        messages[0].Attempts.Should().Be(1);
        messages[0].LastError.Should().NotBeNullOrEmpty();
        messages[0].ProcessedAt.Should().BeNull();
        messages[1].ProcessedAt.Should().BeNull();
        morePending.Should().BeFalse();
    }

    [Fact(DisplayName = "Pending messages should be read with the configured batch size and attempt limit")]
    public async Task Given_Options_When_Executed_Then_ShouldQueryWithThem()
    {
        Pending(0);

        var morePending = await Job().ExecuteAsync(CancellationToken.None);

        await _repository.Received(1).LockPendingAsync(BatchSize, MaxAttempts, Arg.Any<CancellationToken>());
        _published.Should().BeEmpty();
        morePending.Should().BeFalse();
    }

    private OutboxDispatchJob Job() => new(
        _repository,
        _unitOfWork,
        _bus,
        Options.Create(new MessagingOptions { BatchSize = BatchSize, MaxAttempts = MaxAttempts }),
        NullLogger<OutboxDispatchJob>.Instance);

    private List<OutboxMessage> Pending(int count)
    {
        var messages = Enumerable.Range(0, count)
            .Select(i =>
            {
                var integrationEvent = new SaleCancelledIntegrationEvent
                {
                    EventId = Guid.NewGuid(),
                    OccurredAt = DateTime.UtcNow.AddSeconds(i),
                    SaleId = Guid.NewGuid(),
                    SaleNumber = i + 1
                };
                return new OutboxMessage(integrationEvent.EventId, integrationEvent, integrationEvent.OccurredAt);
            })
            .ToList();

        _repository.LockPendingAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(messages);
        return messages;
    }

    // Simulates a message whose type no longer exists (e.g. renamed after it was stored).
    private static void Corrupt(OutboxMessage message) =>
        typeof(OutboxMessage).GetProperty(nameof(OutboxMessage.Type))!.SetValue(message, "Missing.Type, Missing.Assembly");
}
