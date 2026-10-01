using System.Text.Json;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public class OutboxMessage
{
    public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(Guid id, object message, DateTime occurredAt)
    {
        var type = message.GetType();

        Id = id;
        Type = $"{type.FullName}, {type.Assembly.GetName().Name}";
        Payload = JsonSerializer.Serialize(message, type, SerializerOptions);
        OccurredAt = occurredAt;
    }

    public object ToMessage()
    {
        var type = System.Type.GetType(Type, throwOnError: true)!;
        return JsonSerializer.Deserialize(Payload, type, SerializerOptions)
            ?? throw new InvalidOperationException($"Outbox message {Id} has an empty payload.");
    }

    public void MarkProcessed(DateTime processedAt) => ProcessedAt = processedAt;

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error;
    }
}
