namespace Ambev.DeveloperEvaluation.Messaging.Polling;

public interface IPollingJob
{
    Task<bool> ExecuteAsync(CancellationToken cancellationToken);
}
