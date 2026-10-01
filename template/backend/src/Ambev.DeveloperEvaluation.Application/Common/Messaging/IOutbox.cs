namespace Ambev.DeveloperEvaluation.Application.Common.Messaging;

public interface IOutbox
{
    void Add(IIntegrationEvent integrationEvent);
}
