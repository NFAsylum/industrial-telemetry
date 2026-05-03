namespace Telemetry.Infrastructure.Configuration;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}