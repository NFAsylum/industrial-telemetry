namespace Telemetry.Domain;

public record SensorReading
{
    public Guid Id { get; init; }
    public required string SensorId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public double Value { get; init; }
    public required string Unit { get; init; }
}
