using Ambev.DeveloperEvaluation.Application.Common.Messaging;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxStore : IOutbox
{
    private readonly DefaultContext _context;

    public OutboxStore(DefaultContext context)
    {
        _context = context;
    }

    public void Add(IIntegrationEvent integrationEvent) =>
        _context.OutboxMessages.Add(new OutboxMessage(integrationEvent.EventId, integrationEvent, integrationEvent.OccurredAt));
}
