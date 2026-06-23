using Telemetry.Contracts;

namespace Telemetry.Ingestion.Api.Filters;

public class CachedResponse
{
    public required string CorrelationId { get; init; }
    public required IngestionResponse Body { get; init; }
}