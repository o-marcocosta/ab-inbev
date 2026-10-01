namespace Ambev.DeveloperEvaluation.Application.Common.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
