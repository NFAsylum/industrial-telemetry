namespace Telemetry.Contracts;

public record IngestionResponse
{
    public required Guid MessageId { get; init; }
}