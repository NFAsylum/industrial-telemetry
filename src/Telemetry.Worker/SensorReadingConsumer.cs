using MassTransit;
using Telemetry.Contracts;
using Telemetry.Domain;
using Telemetry.Infrastructure;

namespace Telemetry.Worker;

public class SensorReadingConsumer : IConsumer<SensorReadingMessage>
{
    private readonly TelemetryDbContext _db;
    private readonly ILogger<SensorReadingConsumer> _logger;

    public SensorReadingConsumer(TelemetryDbContext db, ILogger<SensorReadingConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SensorReadingMessage> context)
    {
        SensorReadingMessage msg = context.Message;
        
        _logger.LogInformation(
            "Processing reading {MessageId} for sensor {SensorId} at {Timestamp}",
            msg.MessageId,
            msg.SensorId,
            msg.Timestamp);

        SensorReading reading = new SensorReading()
        {
            Id = msg.MessageId,
            SensorId = msg.SensorId,
            Value = msg.Value,
            Unit = msg.Unit,
            Timestamp = msg.Timestamp
        };
        
        _db.SensorReadings.Add(reading);
        await _db.SaveChangesAsync(context.CancellationToken);
        
        _logger.LogInformation("Persisted reading {MessageId} for sensor {SensorId}",
            msg.MessageId,
            msg.SensorId);
    }
}