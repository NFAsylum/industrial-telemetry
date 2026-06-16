namespace Telemetry.Contracts;

public record IngestionRequest
{
    public required string SensorId { get; init; }
    public double Value { get; init; }
    public required string Unit { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
