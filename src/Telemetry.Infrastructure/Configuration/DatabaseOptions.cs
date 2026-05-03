namespace Telemetry.Infrastructure.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public required string ConnectionString { get; init; }
}